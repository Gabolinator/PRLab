using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

internal static class SeedAliases
{
    public static bool AddMissing(IEnumerable<Alias> existing, IEnumerable<Alias> requested, Action<Alias> addAlias)
    {
        var existingKeys = existing.Select(alias => alias.ValueKey).ToHashSet(StringComparer.Ordinal);
        var changed = false;
        foreach (var alias in requested)
        {
            if (existingKeys.Add(alias.ValueKey))
            {
                addAlias(alias);
                changed = true;
            }
        }

        return changed;
    }
}
