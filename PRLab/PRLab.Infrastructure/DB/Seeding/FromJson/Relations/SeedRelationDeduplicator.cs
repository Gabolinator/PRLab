using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations;

public static class SeedRelationDeduplicator
{
    public static IReadOnlyList<SeedRelationItem<TId>> DeduplicateDirected<TId>(
        IReadOnlyCollection<SeedRelationItem<TId>> relations)
        where TId : notnull
    {
        var seen = new HashSet<(TId SourceId, TId TargetId)>();
        var result = new List<SeedRelationItem<TId>>();

        foreach (var relation in relations)
        {
            var key = (relation.SourceId, relation.TargetId);

            if (!seen.Add(key))
            {
                continue;
            }

            result.Add(relation);
        }

        return result;
    }

    public static IReadOnlyList<SeedRelationItem<TId>> DeduplicateUndirected<TId>(
        IReadOnlyCollection<SeedRelationItem<TId>> relations)
        where TId : notnull
    {
        var seen = new HashSet<string>();
        var result = new List<SeedRelationItem<TId>>();

        foreach (var relation in relations)
        {
            var left = relation.SourceId!.ToString();
            var right = relation.TargetId!.ToString();

            var key = string.CompareOrdinal(left, right) <= 0
                ? $"{left}:{right}"
                : $"{right}:{left}";

            if (!seen.Add(key))
            {
                continue;
            }

            result.Add(relation);
        }

        return result;
    }
}