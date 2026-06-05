using PRLab.Domain;

namespace PRLab.Infrastructure.DB.Seeding.Export;


public static class SeedReferences
{
    public static readonly IReadOnlyDictionary<DomainEnum.EntityType, string> TargetAliases =
        new Dictionary<DomainEnum.EntityType, string>
        {
            [DomainEnum.EntityType.Equipment] = "equipment",
            [DomainEnum.EntityType.Muscle] = "muscles",
            [DomainEnum.EntityType.MovementCategory] = "movement-categories",
            [DomainEnum.EntityType.Movement] = "movements",
        };
    
    public static string GetEntityAlias(
        DomainEnum.EntityType entity)
    {
         if(!TargetAliases.TryGetValue(entity, out var targetAlias))
         {
             throw new InvalidOperationException($"No alias for entity {entity} was found.");
         }

         return targetAlias;  
        
    }
    
    public static IReadOnlyDictionary<string, DomainEnum.EntityType> GetTargetAliases(
        IReadOnlyCollection<DomainEnum.EntityType> availableTargets)
    {
        var aliases = new Dictionary<string, DomainEnum.EntityType>(StringComparer.OrdinalIgnoreCase);

        foreach (var target in availableTargets)
        {
            if (!TargetAliases.TryGetValue(target, out var targetAlias))
            {
                continue;
            }

            aliases[targetAlias] = target;
        }

        return aliases;
    }
}