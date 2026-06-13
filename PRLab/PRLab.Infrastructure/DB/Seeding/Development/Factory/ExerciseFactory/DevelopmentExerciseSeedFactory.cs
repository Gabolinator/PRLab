using Microsoft.Extensions.Logging;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Exercise;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory.ExerciseFactory;

public sealed class DevelopmentExerciseSeedFactory(
    IUserService userService,
    ILogger<DevelopmentExerciseSeedFactory> logger) : IExerciseSeedFactory
{
    private User SeedUser => userService.GetSystemAdminUser("Seed");

    public IReadOnlyList<SeedItem<Exercise>> CreateInitialData(MovementSeedCatalog catalog)
    {
        try
        {
            var burpeePullUp = Exercise.NewBuiltIn(
                name: "Burpee Pull-Up",
                description: Description.New(
                    "Burpee directly to Pull-Up",
                    (LocalizationHelper.Language?)null),
                createdBy: SeedUser
            );

            var burpee = catalog.GetRequiredByName("Burpee");

            burpeePullUp.AddBlock(
                movementId: burpee.Id,
                target: WorkTarget.ForReps(1),
                loadTarget: LoadTarget.BodyWeight(),
                changedBy: SeedUser);

            var pullUp = catalog.GetRequiredByName("Pull Up");

            burpeePullUp.AddBlock(
                movementId: pullUp.Id,
                target: WorkTarget.ForReps(1),
                loadTarget: LoadTarget.BodyWeight(),
                changedBy: SeedUser);

            var exercises = new List<Exercise>
            {
                burpeePullUp
            };

            return exercises
                .Select(exercise =>
                    new SeedItem<Exercise>(
                        SeedKeyGenerator.GenerateExerciseKey(exercise),
                        exercise,
                        SeedAction.CreateIfMissing))
                .ToList();
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to create development exercise seed data.");

            throw;
        }
    }

    private IReadOnlyList<SeedItem<Exercise>> CreateExerciseFromMovements(MovementSeedCatalog catalog)
    {
        // Get all movements and make a basic exercise from each.
        // Wait a bit before doing this to avoid noisy default exercises.
        return [];
    }
}