using Microsoft.AspNetCore.Mvc;
using PRLab.API.Controller.Entity;
using PRLab.API.DTO.Equipment;
using PRLab.API.DTO.Exercise;
using PRLab.API.DTO.Movement;
using PRLab.API.DTO.Muscle;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Infrastructure.DB.Repositories.Entity;
using PRLab.Infrastructure.Utilities;
using PRLab.Tests.InfrastructureTests;

namespace PRLab.Tests.APITests;

public sealed class CatalogAliasResponseTests
{
    [Fact]
    public async Task CatalogReadsReturnPersistedAliasesInFullResponses()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();
        var equipment = Equipment.NewBuiltIn("Dumbbell", Description.None());
        var bench = Equipment.NewBuiltIn("Flat Bench", Description.None());
        var movement = Movement.NewBuiltIn(
            "Squat",
            MovementCategory.NewBuiltIn("Strength", "Strength movements.", BaseMovementCategory.Resistance),
            Description.None(),
            WorkTargetType.Repetitions,
            MovementLaterality.Bilateral);
        var exercise = Exercise.FromMovementBuiltIn(movement);
        var muscle = Muscle.New("Quadriceps", null, BodySection.LowerBody, Description.None());
        equipment.AddAlias("Dumb Bell");
        equipment.AddAlias("DB");
        movement.AddAlias("Knee Bend");
        exercise.AddAlias("Air Squat");
        muscle.AddAlias("Quads");

        database.Db.AddRange(equipment, bench, movement, exercise, muscle);
        await database.Db.SaveChangesAsync();
        database.Db.ChangeTracker.Clear();

        var logger = new PRLabLogger();
        var equipmentController = new EquipmentController(new EquipmentRepository(database.Db), null!, logger);
        var movementController = new MovementController(new MovementRepository(database.Db), null!, logger);
        var exerciseController = new ExerciseController(new ExerciseRepository(database.Db), null!, logger);
        var muscleController = new MuscleController(new MuscleRepository(database.Db), null!, logger);

        var equipmentDetail = GetResponse<EquipmentGetDTO>(await equipmentController.GetEquipment(equipment.Id.Value));
        var equipmentList = GetResponse<IReadOnlyCollection<EquipmentGetDTO>>(await equipmentController.GetAllEquipments());
        Assert.Equal(["DB", "Dumb Bell"], equipmentDetail.Aliases);
        Assert.Equal(equipmentDetail.Aliases, Assert.Single(equipmentList, entry => entry.Id == equipment.Id).Aliases);
        Assert.Empty(Assert.Single(equipmentList, entry => entry.Id == bench.Id).Aliases);

        var movementDetail = GetResponse<MovementGetDTO>(await movementController.GetMovement(movement.Id.Value));
        var movementList = GetResponse<IReadOnlyCollection<MovementGetDTO>>(await movementController.GetAllMovements());
        Assert.Equal("Knee Bend", Assert.Single(movementDetail.Aliases));
        Assert.Equal(movementDetail.Aliases, Assert.Single(movementList).Aliases);

        var exerciseDetail = GetResponse<ExerciseGetDTO>(await exerciseController.GetExercise(exercise.Id.Value));
        var exerciseList = GetResponse<IReadOnlyCollection<ExerciseGetDTO>>(await exerciseController.GetAllExercises());
        var exercisesByMovement = GetResponse<IReadOnlyCollection<ExerciseGetDTO>>(
            await exerciseController.GetExercisesByMovement(movement.Id.Value));
        Assert.Equal("Air Squat", Assert.Single(exerciseDetail.Aliases));
        Assert.Equal(exerciseDetail.Aliases, Assert.Single(exerciseList).Aliases);
        Assert.Equal(exerciseDetail.Aliases, Assert.Single(exercisesByMovement).Aliases);

        var muscleDetail = GetResponse<MuscleGetDTO>(await muscleController.GetMuscle(muscle.Id.Value));
        var muscleList = GetResponse<IReadOnlyCollection<MuscleGetDTO>>(await muscleController.GetAllMuscles());
        Assert.Equal("Quads", Assert.Single(muscleDetail.Aliases));
        Assert.Equal(muscleDetail.Aliases, Assert.Single(muscleList).Aliases);
    }

    private static T GetResponse<T>(IActionResult response) =>
        Assert.IsAssignableFrom<T>(Assert.IsType<OkObjectResult>(response).Value);
}
