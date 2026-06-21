using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

public static class WorkoutQueryExtensions
{
    public static IQueryable<Workout> WithFullAggregate(
        this IQueryable<Workout> query)
    {
        return query
            .AsSplitQuery()
            .Include(workout => workout.Description)
            .ThenInclude(description => description.Translations)

            .Include(workout => workout.Blocks)
            .ThenInclude(assignment => assignment.WorkoutBlock)
            .ThenInclude(workoutBlock => workoutBlock.Segments)
            .ThenInclude(segment => segment.Steps)
            .ThenInclude(step => step.Exercise)

            .Include(workout => workout.Blocks)
            .ThenInclude(assignment => assignment.WorkoutBlock)
            .ThenInclude(workoutBlock => workoutBlock.Segments)
            .ThenInclude(segment => segment.Steps)
            .ThenInclude(step => step.Exercise)
            .ThenInclude(exercise => exercise.Description)
            .ThenInclude(description => description.Translations);
    }

    public static IQueryable<Workout> ActiveOnly(
        this IQueryable<Workout> query)
    {
        return query
            .Where(workout => !workout.Audit.IsDeleted);
    }

    public static IQueryable<Workout> ForFullRead(
        this IQueryable<Workout> query)
    {
        return query
            .WithFullAggregate()
            .ActiveOnly()
            .AsNoTracking();
    }

    public static IQueryable<Workout> ForFullWrite(
        this IQueryable<Workout> query)
    {
        return query
            .WithFullAggregate()
            .ActiveOnly();
    }
}