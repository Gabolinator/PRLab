using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Workout;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public class WorkoutSeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IWorkoutSeedFactory seedFactory,
    IAppLogger logger) : EntitySeederBase(db, logger)
{
    public override string Name => "DevelopmentWorkoutSeed";

    public override string Version => "1.0.0";

    public override EntityType EntityType => EntityType.Workout;

    public override User SeedUser => userService.GetSystemAdminUser("Seed");

    protected override async Task<IReadOnlyList<SeedChange>> SeedEntityAsync(CancellationToken ct)
    {
        var catalog = await SeedCatalogBuilder.CreateExerciseCatalog(db, ct);
        var workoutSeedItems = seedFactory.CreateInitialData(catalog);

        var changes = new List<SeedChange>();

        foreach (var workoutSeedItem in workoutSeedItems)
        {
            var result = await ApplyWorkoutSeedItem(workoutSeedItem, ct);

            if (result.change is not null)
            {
                changes.Add(result.change);
            }
        }

        await db.SaveChangesAsync(ct);

        return changes;
    }

    private async Task<(Workout? entity, SeedChange? change)> ApplyWorkoutSeedItem(
        SeedItem<Workout> workoutSeedItem,
        CancellationToken ct)
    {
        if (workoutSeedItem.Action == SeedAction.Ignore)
        {
            return (null, null);
        }

        var seedWorkout = workoutSeedItem.Entity;

        var existingWorkout = await db.Workouts
            .AsSplitQuery()
            .Include(workout => workout.Description)
                .ThenInclude(description => description.Translations)
            .Include(workout => workout.Blocks)
                .ThenInclude(blockAssignment => blockAssignment.WorkoutBlock)
                    .ThenInclude(workoutBlock => workoutBlock.Segments)
                        .ThenInclude(segment => segment.Steps)
                            .ThenInclude(step => step.Exercise)
            .FirstOrDefaultAsync(
                workout => workout.NameKey == seedWorkout.NameKey,
                ct);

        if (existingWorkout is null)
        {
            await db.Workouts.AddAsync(seedWorkout, ct);

            logger.Log($"Seeded - {EntityType} : {seedWorkout.NameKey}");

            return (
                seedWorkout,
                new SeedChange(
                    seedWorkout.NameKey,
                    SeedChangeType.Created));
        }

        if (workoutSeedItem.Action == SeedAction.CreateIfMissing)
        {
            return (existingWorkout, null);
        }

        logger.Log($"Seeder Updating - {EntityType} : {seedWorkout.NameKey}");

        // var hasChanged = existingWorkout.Update(
        //     WorkoutUpdate.FromWorkout(
        //         seedWorkout,
        //         SeedUser));
        var hasChanged = true;
        return hasChanged
            ? (
                existingWorkout,
                new SeedChange(
                    seedWorkout.NameKey,
                    SeedChangeType.Updated))
            : (existingWorkout, null);
    }
}