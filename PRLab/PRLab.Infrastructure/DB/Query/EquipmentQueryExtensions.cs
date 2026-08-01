using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

public static class EquipmentQueryExtensions
{
    public static IQueryable<Equipment> WithFullAggregate(
        this IQueryable<Equipment> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .Include(equipment => equipment.Description)
            .ThenInclude(description => description.Translations);
    }

    public static IQueryable<Equipment> ActiveOnly(
        this IQueryable<Equipment> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .Where(equipment => !equipment.Audit.IsDeleted);
    }

    public static IQueryable<Equipment> ForFullRead(
        this IQueryable<Equipment> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .WithFullAggregate()
            .ActiveOnly()
            .AsNoTracking();
    }

    public static IQueryable<Equipment> ForFullWrite(
        this IQueryable<Equipment> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query
            .WithFullAggregate()
            .ActiveOnly();
    }
}