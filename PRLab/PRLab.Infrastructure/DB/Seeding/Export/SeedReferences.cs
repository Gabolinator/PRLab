using PRLab.Domain;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.DB.Seeding.Export;


public static class SeedReferences
{
    public static readonly IReadOnlyDictionary<DomainEnum.EntityType, string> TargetAliases =
        new Dictionary<DomainEnum.EntityType, string>
        {
            [DomainEnum.EntityType.Equipment] = "equipment",
            [DomainEnum.EntityType.Muscle] = "muscle",
            [DomainEnum.EntityType.MovementCategory] = "movement-category",
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

    public static IReadOnlyList<DomainEnum.EntityType> ExpandSeedDependencies(
        IReadOnlyCollection<DomainEnum.EntityType> entities, IAppLogger? logger)
    {
        var expanded = entities.ToHashSet();

        if (expanded.Contains(DomainEnum.EntityType.Muscle))
        {
            expanded.Add(DomainEnum.EntityType.MuscleAntagonist);
        }
        
        if (expanded.Contains(DomainEnum.EntityType.Movement))
        {
            logger?.LogWarning("No dependencies yet for Movement");
        }

        //todo add other dependencies
        
        return expanded.ToList();
    }
}