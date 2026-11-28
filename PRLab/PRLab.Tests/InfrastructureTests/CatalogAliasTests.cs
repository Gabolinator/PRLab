using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.UserService;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Enum.Equipment;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Update;
using PRLab.Infrastructure.DB.Repositories.Entity;
using PRLab.Infrastructure.DB.Seeding.EntitySeeders;
using PRLab.Infrastructure.DB.Seeding.Export.Exporter;
using PRLab.Infrastructure.DB.Seeding.FromJson.Factory;
using PRLab.Infrastructure.Utilities;
using PRLab.Infrastructure.WorkoutImport;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;
using PRLab.Infrastructure.WorkoutImport.Interpreter;

namespace PRLab.Tests.InfrastructureTests;

public sealed class CatalogAliasTests
{
    [Fact]
    public async Task SharedAliasPersistsAndResolvesAcrossAllFourAggregateTypes()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();
        var equipment = Equipment.NewBuiltIn("Dumbbell", Description.None());
        var movement = CreateMovement();
        var exercise = Exercise.FromMovementBuiltIn(movement);
        var muscle = Muscle.New("Quadriceps", null, BodySection.LowerBody, Description.None());
        var sharedAlias = Alias.New(" QD ");
        equipment.AddAlias(sharedAlias);
        movement.AddAlias(sharedAlias);
        exercise.AddAlias(sharedAlias);
        muscle.AddAlias(sharedAlias);

        Assert.Throws<InvalidOperationException>(() => equipment.AddAlias("qd"));
        Assert.Throws<InvalidOperationException>(() => movement.AddAlias(" q-d "));
        Assert.Throws<InvalidOperationException>(() => exercise.AddAlias("QD"));
        Assert.Throws<InvalidOperationException>(() => muscle.AddAlias(" q_d "));
        Assert.Throws<ArgumentException>(() => equipment.AddAlias(" -- "));

        database.Db.AddRange(equipment, movement, exercise, muscle);
        await database.Db.SaveChangesAsync();
        database.Db.ChangeTracker.Clear();

        var equipmentRepository = new EquipmentRepository(database.Db);
        var equipmentMatch = await equipmentRepository.GetByNameAsync(" q-d ", default);
        Assert.Equal(equipment.Id, equipmentMatch?.Id);
        Assert.Equal("QD", Assert.Single(equipmentMatch!.Aliases).Alias.Value);
        Assert.Equal(movement.Id, (await new MovementRepository(database.Db).GetByNameAsync("qd", default))?.Id);
        Assert.Equal(exercise.Id, (await new ExerciseRepository(database.Db).GetByNameAsync("QD", default))?.Id);
        Assert.Equal(muscle.Id, (await new MuscleRepository(database.Db).GetByNameAsync("qd", default))?.Id);
        Assert.Equal(1, await database.Db.Aliases.CountAsync());

        var trackedEquipment = await equipmentRepository.GetTrackedByIdAsync(equipment.Id, default);
        Assert.True(trackedEquipment!.RemoveAlias("qd"));
        await equipmentRepository.UpdateAsync(trackedEquipment, default);
        database.Db.ChangeTracker.Clear();

        Assert.Null(await equipmentRepository.GetByNameAsync("qd", default));
        Assert.Equal(exercise.Id, (await new ExerciseRepository(database.Db).GetByNameAsync("qd", default))?.Id);
    }

    [Fact]
    public async Task CanonicalMatchWinsAndAmbiguousAliasesDoNotPickAnArbitraryOwner()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();
        var dumbbell = Equipment.NewBuiltIn("Dumbbell", Description.None());
        var barbell = Equipment.NewBuiltIn("Barbell", Description.None());
        var kettlebell = Equipment.NewBuiltIn("Kettlebell", Description.None());
        dumbbell.AddAlias("DB");
        barbell.AddAlias("DB");
        kettlebell.AddAlias("Dumbbell");
        database.Db.AddRange(dumbbell, barbell, kettlebell);
        await database.Db.SaveChangesAsync();
        database.Db.ChangeTracker.Clear();

        var repository = new EquipmentRepository(database.Db);
        Assert.Equal(dumbbell.Id, (await repository.GetByNameAsync("Dumb Bell", default))?.Id);
        Assert.Null(await repository.GetByNameAsync("DB", default));
        Assert.Equal(3, await database.Db.Aliases.CountAsync());
    }

    [Fact]
    public async Task DatabaseRejectsTwoAliasIdsWithTheSameOwnerAndNormalizedValue()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();
        var equipment = Equipment.NewBuiltIn("Dumbbell", Description.None());
        equipment.AddAlias("DB");
        var duplicate = Alias.New(" db ");
        database.Db.AddRange(equipment, duplicate);
        await database.Db.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(() =>
            database.Db.Database.ExecuteSqlInterpolatedAsync(
                $"""INSERT INTO "EquipmentAlias" ("EquipmentId", "AliasId", "AliasValueKey") VALUES ({equipment.Id.Value}, {duplicate.Id.Value}, {duplicate.ValueKey})"""));
        Assert.Equal(2067, exception.SqliteExtendedErrorCode); // SQLITE_CONSTRAINT_UNIQUE
    }

    [Fact]
    public async Task EquipmentExportAndReseedRoundTripAliasesWithoutRemovingExistingOnes()
    {
        await using var source = await RepositoryTestDatabase.CreateAsync();
        var equipment = Equipment.NewBuiltIn("Dumbbell", Description.None());
        equipment.AddAlias("DB");
        equipment.AddAlias("Dumb Bell");
        source.Db.Equipments.Add(equipment);
        await source.Db.SaveChangesAsync();

        var directory = Directory.CreateTempSubdirectory("prlab-alias-seed-");
        var config = new AliasSeedConfig(directory.FullName);
        var exporter = new EquipmentJsonDataExporter(source.Db, config);
        try
        {
            await exporter.ExportAsync();
            var users = new TestUserProvider();
            var factory = new JsonEquipmentSeedFactory(users, config);
            var seed = Assert.Single(factory.CreateInitialData(SeedExecutionOptions.FullReseed));
            Assert.Equal(new[] { "DB", "Dumb Bell" }, seed.Entity.Aliases.Select(relation => relation.Alias.Value));

            await using var destination = await RepositoryTestDatabase.CreateAsync();
            var existing = Equipment.NewBuiltIn("Dumbbell", Description.None());
            existing.AddAlias("Hand Weight");
            destination.Db.Equipments.Add(existing);
            await destination.Db.SaveChangesAsync();

            var seeder = new EquipmentSeeder(destination.Db, users, factory, new PRLabLogger());
            await seeder.SeedAsync(SeedExecutionOptions.FullReseed);
            await seeder.SeedAsync(SeedExecutionOptions.FullReseed);
            destination.Db.ChangeTracker.Clear();

            var repository = new EquipmentRepository(destination.Db);
            var match = await repository.GetByNameAsync("DB", default);
            Assert.Equal(existing.Id, match?.Id);
            Assert.Equal(3, match!.Aliases.Count);
        }
        finally
        {
            File.Delete(exporter.DefaultFilePath);
            directory.Delete();
        }
    }

    [Fact]
    public async Task EquipmentConfigurationTypesPersistAndCanBeReplaced()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();
        var equipment = Equipment.NewBuiltIn(
            "Adjustable Rack",
            Description.None(),
            supportedConfigurationTypes: [EquipmentConfigurationType.Height]);
        var repository = new EquipmentRepository(database.Db);
        await repository.CreateAsync(equipment, default);
        database.Db.ChangeTracker.Clear();

        var tracked = await repository.GetTrackedByIdAsync(equipment.Id, default);
        Assert.NotNull(tracked);
        Assert.Equal(EquipmentConfigurationType.Height, Assert.Single(tracked.SupportedConfigurationTypes));
        tracked.Update(new EquipmentUpdate { SupportedConfigurationTypes = [EquipmentConfigurationType.Angle, EquipmentConfigurationType.Length] });
        await repository.UpdateAsync(tracked, default);
        database.Db.ChangeTracker.Clear();

        var reloaded = await repository.GetByIdAsync(equipment.Id, default);
        Assert.NotNull(reloaded);
        Assert.Equal([EquipmentConfigurationType.Length, EquipmentConfigurationType.Angle], reloaded.SupportedConfigurationTypes);
    }

    [Fact]
    public async Task ImportLoadsExerciseAndEquipmentAliasesWithoutProfileAliases()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();
        var movement = CreateMovement();
        var exercise = Exercise.FromMovementBuiltIn(movement);
        exercise.AddAlias("Knee Dip");
        var equipment = Equipment.NewBuiltIn("Dumbbell", Description.None());
        equipment.AddAlias("DB");
        var platform = Equipment.NewBuiltIn(
            "Training Platform",
            Description.None(),
            supportedConfigurationTypes: [EquipmentConfigurationType.Height, EquipmentConfigurationType.Angle]);
        movement.AddRequiredEquipmentOption(platform.Id, "standing surface");
        database.Db.AddRange(movement, exercise, equipment, platform);
        await database.Db.SaveChangesAsync();
        database.Db.ChangeTracker.Clear();

        var profile = new WorkoutImportProfile
        {
            Name = "Test",
            SourceProfiles =
            [
                new ExcelWorkoutImportSourceProfile
                {
                    Columns =
                    [
                        new() { Property = WorkoutImportProperty.Exercise, CellIndex = 1 },
                        new() { Property = WorkoutImportProperty.Details, CellIndex = 2 }
                    ]
                }
            ]
        };
        var logger = new PRLabLogger();
        var contextFactory = new WorkoutImportContextFactory(
            new ExerciseRepository(database.Db),
            new EquipmentRepository(database.Db),
            new TestUserProvider(),
            new TestProfileProvider(profile),
            logger);
        var context = await contextFactory.CreateAsync();
        var exerciseReference = Assert.Single(context.Exercises);
        Assert.Equal("Knee Dip", Assert.Single(exerciseReference.Aliases));
        Assert.Equal(platform.Id, Assert.Single(exerciseReference.EquipmentIds));
        Assert.Equal("DB", Assert.Single(Assert.Single(context.Equipment, reference => reference.Id == equipment.Id).Aliases));
        var platformReference = Assert.Single(context.Equipment, reference => reference.Id == platform.Id);
        Assert.Equal([EquipmentConfigurationType.Height, EquipmentConfigurationType.Angle], platformReference.SupportedConfigurationTypes);

        var source = new ExtractedExcelWorkoutSource
        {
            SourceType = WorkoutImportSourceType.File,
            NormalizedText = string.Empty,
            Sheets =
            [
                new ExtractedWorkoutSheet
                {
                    Name = "Test",
                    RawText = string.Empty,
                    Rows =
                    [
                        new()
                        {
                            Cells =
                            [
                                new() { Value = "2026-11-28" },
                                new() { ColumnIndex = 1, Value = "knee dip" },
                                new() { ColumnIndex = 2, Value = "2 db | 61cm" }
                            ]
                        }
                    ]
                }
            ]
        };
        var imported = await new CoachExcelWorkoutImportInterpreter(logger).InterpretAsync(source, context);
        var row = Assert.Single(Assert.Single(imported).Rows);
        Assert.Equal(exercise.Id, Assert.Single(row.ExerciseParts).ExerciseId);
        var interpretation = Assert.Single(row.ExerciseInterpretations);
        var equipmentItems = interpretation.SemanticItems.OfType<WorkoutEquipmentSemantic>().ToList();
        var matchedEquipment = Assert.Single(equipmentItems, item => item.Quantity.HasValue);
        Assert.Equal(equipment.Id, matchedEquipment.EquipmentId);
        Assert.Equal(2, matchedEquipment.Quantity);
        var height = Assert.Single(equipmentItems, item => item.HeightCentimeters.HasValue);
        Assert.Equal(platform.Id, height.EquipmentId);
        Assert.Equal(61m, height.HeightCentimeters);
    }

    private static Movement CreateMovement() =>
        Movement.NewBuiltIn(
            "Squat",
            MovementCategory.NewBuiltIn("Strength", "Strength movements.", BaseMovementCategory.Resistance),
            Description.None(),
            WorkTargetType.Repetitions,
            MovementLaterality.Bilateral);

    private sealed record AliasSeedConfig(string SeedFileDirectory) : ISeedingConfig
    {
        public SeedingSource Source => SeedingSource.JsonFiles;
    }

    private sealed class TestUserProvider : ISystemUserProvider, ICurrentUserService
    {
        private readonly User user = User.Existing(UserId.New(), "Alias tests", UserRole.Admin);

        public bool IsAuthenticated => true;

        public User GetSystemAdminUser(string? name = null) => user;

        public UserId GetRequiredUserId() => user.Id;

        public Task<User?> GetCurrentUserAsync(CancellationToken ct = default) => Task.FromResult<User?>(user);

        public Task<User> GetRequiredCurrentUserAsync(CancellationToken ct = default) => Task.FromResult(user);
    }

    private sealed class TestProfileProvider(WorkoutImportProfile profile) : IWorkoutImportProfileProvider
    {
        public Task<WorkoutImportProfile> GetAsync(string? profileName = null, CancellationToken ct = default) =>
            Task.FromResult(profile);
    }
}
