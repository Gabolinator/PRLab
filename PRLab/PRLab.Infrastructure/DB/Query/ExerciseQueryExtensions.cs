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
            .ThenInclude(exerciseStep => exerciseStep.Movement);
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