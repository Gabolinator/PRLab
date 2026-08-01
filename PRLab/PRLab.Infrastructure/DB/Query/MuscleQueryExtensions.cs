using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

public static class MuscleQueryExtensions
{
    public static IQueryable<Muscle> WithFullAggregate(
        this IQueryable<Muscle> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .AsSplitQuery()
            .Include(muscle => muscle.Description)
            .ThenInclude(description => description.Translations)
            .Include(muscle => muscle.Functions)
            .Include(muscle => muscle.Antagonists)
            .ThenInclude(antagonist => antagonist.AntagonistMuscle)
            .ThenInclude(antagonistMuscle => antagonistMuscle.Description)
            .ThenInclude(description => description.Translations);
    }

    public static IQueryable<Muscle> ActiveOnly(
        this IQueryable<Muscle> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .Where(muscle => !muscle.Audit.IsDeleted);
    }

    public static IQueryable<Muscle> ForFullRead(
        this IQueryable<Muscle> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .WithFullAggregate()
            .ActiveOnly()
            .AsNoTracking();
    }

    public static IQueryable<Muscle> ForFullWrite(
        this IQueryable<Muscle> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .WithFullAggregate()
            .ActiveOnly();
    }
}