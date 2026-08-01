using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

public static class DescriptionQueryExtensions
{
    public static IQueryable<Description> WithFullAggregate(
        this IQueryable<Description> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .Include(description => description.Translations);
    }

    public static IQueryable<Description> ForFullRead(
        this IQueryable<Description> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .WithFullAggregate()
            .AsNoTracking();
    }

    public static IQueryable<Description> ForFullWrite(
        this IQueryable<Description> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .WithFullAggregate();
    }
}