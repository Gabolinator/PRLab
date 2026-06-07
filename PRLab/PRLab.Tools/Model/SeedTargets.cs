using PRLab.Domain;


namespace PRLab.Tools.Model;

public static class SeedTargets
{
    public const string All = "all";
    
    public static bool TryGetTargetAlias(
        DomainEnum.EntityType entityType,
        PRToolConfig config,
        out string targetAlias)
    {
        targetAlias = string.Empty;

        var match = config.TargetAliases
            .FirstOrDefault(alias => alias.Value == entityType);

        if (string.IsNullOrWhiteSpace(match.Key))
        {
            return false;
        }

        targetAlias = match.Key;
        return true;
    }
    
    public static bool TryGetTarget(
        string? target,
        PRToolConfig config,
        out string normalizedTarget,
        out DomainEnum.EntityType entityType)
    {
        normalizedTarget = string.Empty;
        entityType = default;

        if (string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        normalizedTarget = target.Trim().ToLowerInvariant();

        return config.TargetAliases.TryGetValue(
            normalizedTarget,
            out entityType);
    }
}