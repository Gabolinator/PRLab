using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

public static class ExerciseQueryExtensions
{
    public static IQueryable<Exercise> WithFullAggregate(
        this IQueryable<Exercise> query)
    {
        return query
            .AsSplitQuery()
            .Include(exercise => exercise.Description)
            .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
            .ThenInclude(block => block.Movement)
            .ThenInclude(movement => movement.Description)
            .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
            .ThenInclude(block => block.Movement)
            .ThenInclude(movement => movement.MovementCategory)
            .ThenInclude(movementCategory => movementCategory.Description)
            .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
            .ThenInclude(block => block.Movement)
            .ThenInclude(movement => movement.Patterns)
            .Include(exercise => exercise.Steps)
            .ThenInclude(block => block.Movement)
            .ThenInclude(movement => movement.Muscles)
            .ThenInclude(movementMuscle => movementMuscle.Muscle)
            .ThenInclude(muscle => muscle.Description)
            .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
            .ThenInclude(block => block.Movement)
            .ThenInclude(movement => movement.EquipmentRequirements)
            .ThenInclude(requirement => requirement.Equipment)
            .ThenInclude(equipment => equipment.Description)
            .ThenInclude(description => description.Translations);
    }

    public static IQueryable<Exercise> ActiveOnly(
        this IQueryable<Exercise> query)
    {
        return query
            .Where(exercise => !exercise.Audit.IsDeleted);
    }

    public static IQueryable<Exercise> ForFullRead(
        this IQueryable<Exercise> query)
    {
        return query
            .WithFullAggregate()
            .ActiveOnly()
            .AsNoTracking();
    }

    public static IQueryable<Exercise> ForFullWrite(
        this IQueryable<Exercise> query)
    {
        return query
            .WithFullAggregate()
            .ActiveOnly();
    }
}