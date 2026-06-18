using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

public static class MovementQueryExtensions
{
    public static IQueryable<Movement> WithFullAggregate(
        this IQueryable<Movement> query)
    {
        return query
            .AsSplitQuery()
            .Include(movement => movement.Description)
            .ThenInclude(description => description.Translations)
            .Include(movement => movement.MovementCategory)
            .ThenInclude(movementCategory => movementCategory.Description)
            .ThenInclude(description => description.Translations)
            .Include(movement => movement.VariantOf)
            .Include(movement => movement.Variants)
            .Include(movement => movement.Patterns)
            .Include(movement => movement.AllowedWorkTargets)
            .Include(movement => movement.Muscles)
            .ThenInclude(movementMuscle => movementMuscle.Muscle)
            .ThenInclude(muscle => muscle.Description)
            .ThenInclude(description => description.Translations)
            .Include(movement => movement.EquipmentRequirements)
            .ThenInclude(requirement => requirement.Equipment)
            .ThenInclude(equipment => equipment.Description)
            .ThenInclude(description => description.Translations);
    }

    public static IQueryable<Movement> ActiveOnly(
        this IQueryable<Movement> query)
    {
        return query
            .Where(movement => !movement.Audit.IsDeleted);
    }

    public static IQueryable<Movement> ForFullRead(
        this IQueryable<Movement> query)
    {
        return query
            .WithFullAggregate()
            .ActiveOnly()
            .AsNoTracking();
    }

    public static IQueryable<Movement> ForFullWrite(
        this IQueryable<Movement> query)
    {
        return query
            .WithFullAggregate()
            .ActiveOnly();
    }
}