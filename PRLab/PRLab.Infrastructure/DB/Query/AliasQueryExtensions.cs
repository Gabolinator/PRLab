using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

internal static class AliasQueryExtensions
{
    public static IQueryable<Muscle> WithAliases(this IQueryable<Muscle> query) =>
        query.Include(entity => entity.Aliases).ThenInclude(relation => relation.Alias);

    public static IQueryable<Movement> WithAliases(this IQueryable<Movement> query) =>
        query.Include(entity => entity.Aliases).ThenInclude(relation => relation.Alias);

    public static IQueryable<Exercise> WithAliases(this IQueryable<Exercise> query) =>
        query.Include(entity => entity.Aliases).ThenInclude(relation => relation.Alias);

    public static IQueryable<Equipment> WithAliases(this IQueryable<Equipment> query) =>
        query.Include(entity => entity.Aliases).ThenInclude(relation => relation.Alias);
}
