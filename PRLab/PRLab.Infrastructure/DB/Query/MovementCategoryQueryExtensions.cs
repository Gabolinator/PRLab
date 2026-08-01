using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

public static class MovementCategoryQueryExtensions
{
    public static IQueryable<MovementCategory> WithFullAggregate(
        this IQueryable<MovementCategory> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .Include(movementCategory => movementCategory.Description)
            .ThenInclude(description => description.Translations);
    }

    public static IQueryable<MovementCategory> ActiveOnly(
        this IQueryable<MovementCategory> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .Where(movementCategory =>
                !movementCategory.Audit.IsDeleted);
    }

    public static IQueryable<MovementCategory> ForFullRead(
        this IQueryable<MovementCategory> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .WithFullAggregate()
            .ActiveOnly()
            .AsNoTracking();
    }

    public static IQueryable<MovementCategory> ForFullWrite(
        this IQueryable<MovementCategory> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .WithFullAggregate()
            .ActiveOnly();
    }
}