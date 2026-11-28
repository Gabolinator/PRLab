using Microsoft.EntityFrameworkCore;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Helpers;

public static class SeedCatalogBuilder
{
    public static async Task<EquipmentSeedCatalog> CreateEquipmentCatalog(
        PRLabPgDBContext db,
        CancellationToken ct,
        bool track = true,
        bool includeAliases = false)
    {
        var query = db.Equipments.ForFullRead();
        if (includeAliases)
        {
            query = query.WithAliases();
        }

        var equipments = await query
            .OrderBy(equipment => equipment.Name)
            .ToListAsync(ct);

        return new EquipmentSeedCatalog(
            new EntityCatalog<EquipmentId, Equipment>(
                equipments,
                equipment => equipment.Id,
                equipment => equipment.NameKey));
    }

    public static async Task<MuscleSeedCatalog> CreateMuscleCatalog(
        PRLabPgDBContext db,
        CancellationToken ct,
        bool track = true,
        bool includeAliases = false)
    {
        var query = track ? db.Muscles.ForFullWrite() : db.Muscles.ForFullRead();
        if (includeAliases)
        {
            query = query.WithAliases();
        }

        var muscles = await query
            .OrderBy(muscle => muscle.Name)
            .ToListAsync(ct);

        return new MuscleSeedCatalog(
            new EntityCatalog<MuscleId, Muscle>(
                muscles,
                muscle => muscle.Id,
                muscle => muscle.NameKey));
    }

    public static async Task<MovementCategorySeedCatalog> CreateMovementCategoryCatalog(
        PRLabPgDBContext db,
        CancellationToken ct,
        bool track = false)
    {
        var query = track
            ? db.MovementCategories.ForFullWrite().AsTracking()
            : db.MovementCategories.ForFullRead();

        var movementCategories = await query
            .OrderBy(movementCategory => movementCategory.Name)
            .ToListAsync(ct);

        return new MovementCategorySeedCatalog(
            new EntityCatalog<MovementCategoryId, MovementCategory>(
                movementCategories,
                movementCategory => movementCategory.Id,
                movementCategory => movementCategory.NameKey));
    }

    public static async Task<MovementSeedCatalog> CreateMovementCatalog(
        PRLabPgDBContext db,
        CancellationToken ct,
        bool includeAliases = false)
    {
        var query = db.Movements.ForFullRead();
        if (includeAliases)
        {
            query = query.WithAliases();
        }

        var movements = await query
            .OrderBy(movement => movement.Name)
            .ToListAsync(ct);

        return new MovementSeedCatalog(
            new EntityCatalog<MovementId, Movement>(
                movements,
                movement => movement.Id,
                movement => movement.NameKey));
    }


    public static async Task<ExerciseSeedCatalog> CreateExerciseCatalog(
        PRLabPgDBContext db,
        CancellationToken ct,
        bool includeAliases = false)
    {
        var query = db.Exercises.ForFullRead();
        if (includeAliases)
        {
            query = query.WithAliases();
        }

        var exercises = await query
            .OrderBy(exercise => exercise.Name)
            .ToListAsync(ct);

        return new ExerciseSeedCatalog(
            new EntityCatalog<ExerciseId, Exercise>(
                exercises,
                exercise => exercise.Id,
                exercise => exercise.NameKey));
    }

    public static async Task<WorkoutSeedCatalog> CreateWorkoutCatalog(
        PRLabPgDBContext db,
        CancellationToken ct)
    {
        var workouts = await db.Workouts
            .ForFullRead()
            .OrderBy(workout => workout.Name)
            .ToListAsync(ct);

        return new WorkoutSeedCatalog(
            new EntityCatalog<WorkoutId, Workout>(
                workouts,
                workout => workout.Id,
                workout => workout.NameKey));
    }
}
