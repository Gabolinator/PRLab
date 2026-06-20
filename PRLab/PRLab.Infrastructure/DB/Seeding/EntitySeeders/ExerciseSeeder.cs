using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Exercise;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public class ExerciseSeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IExerciseSeedFactory seedFactory,
    IAppLogger logger) : EntitySeederBase(db, logger)
{
    public override string Name => "DevelopmentExerciseSeed";

    public override string Version => "1.0.1";

    public override EntityType EntityType => EntityType.Exercise;

    public override User SeedUser => userService.GetSystemAdminUser("Seed");

    protected override async Task<IReadOnlyList<SeedChange>> SeedEntityAsync(CancellationToken ct)
    {
        var movementCatalog = await SeedCatalogBuilder.CreateMovementCatalog(db, ct);

        var exerciseSeedItems = seedFactory.CreateInitialData(movementCatalog);

        var changes = new List<SeedChange>();

        foreach (var exerciseSeedItem in exerciseSeedItems)
        {
            var result = await ApplyExerciseSeedItem(exerciseSeedItem, ct);

            if (result.change is not null)
            {
                changes.Add(result.change);
            }
        }

        await db.SaveChangesAsync(ct);

        return changes;
    }

    private async Task<(Exercise? entity, SeedChange? change)> ApplyExerciseSeedItem(
        SeedItem<Exercise> exerciseSeedItem,
        CancellationToken ct)
    {
        if (exerciseSeedItem.Action == SeedAction.Ignore)
        {
            return (null, null);
        }

        var seedExercise = exerciseSeedItem.Entity;

        var existingExercise = await db.Exercises
            .AsSplitQuery()
            .Include(exercise => exercise.Description)
                .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
                .ThenInclude(exerciseBlock => exerciseBlock.Movement)
            .FirstOrDefaultAsync(
                exercise => exercise.NameKey == seedExercise.NameKey,
                ct);

        if (existingExercise is null)
        {
            await db.Exercises.AddAsync(seedExercise, ct);

            logger.Log($"Seeded - {EntityType} : {seedExercise.NameKey}");

            return (
                seedExercise,
                new SeedChange(
                    seedExercise.NameKey,
                    SeedChangeType.Created));
        }

        if (exerciseSeedItem.Action == SeedAction.CreateIfMissing)
        {
            return (existingExercise, null);
        }

        logger.Log($"Seeder Updating - {EntityType} : {seedExercise.NameKey}");

        var hasChanged = existingExercise.Update(
            ExerciseUpdate.FromExercise(
                seedExercise,
                (LocalizationHelper.Language?) null,
                SeedUser));

        return hasChanged
            ? (
                existingExercise,
                new SeedChange(
                    seedExercise.NameKey,
                    SeedChangeType.Updated))
            : (existingExercise, null);
    }
}