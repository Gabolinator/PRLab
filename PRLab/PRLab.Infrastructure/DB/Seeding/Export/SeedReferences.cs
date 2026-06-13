using PRLab.Domain;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Enum.System;

namespace PRLab.Infrastructure.DB.Seeding.Export;


public static class SeedReferences
{
    public static readonly IReadOnlyDictionary<EntityType, string> TargetAliases =
        new Dictionary<EntityType, string>
        {
            [EntityType.Equipment] = "equipment",
            [EntityType.Muscle] = "muscle",
            [EntityType.MovementCategory] = "movement-category",
            [EntityType.Movement] = "movement",
            [EntityType.Exercise] = "exercise",
            
        };
    
    public static string GetEntityAlias(
        EntityType entity)
    {
         if(!TargetAliases.TryGetValue(entity, out var targetAlias))
         {
             throw new InvalidOperationException($"No alias for entity {entity} was found.");
         }

         return targetAlias;  
        
    }
    
    public static IReadOnlyDictionary<string, EntityType> GetTargetAliases(
        IReadOnlyCollection<EntityType> availableTargets)
    {
        var aliases = new Dictionary<string, EntityType>(StringComparer.OrdinalIgnoreCase);

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

    public static IReadOnlyList<EntityType> ExpandSeedDependencies(
        IReadOnlyCollection<EntityType> entities, IAppLogger? logger)
    {
        var expanded = entities.ToHashSet();

        if (expanded.Contains(EntityType.Muscle))
        {
            expanded.Add(EntityType.MuscleAntagonist);
        }
        
        if (expanded.Contains(EntityType.Movement))
        {
          //  logger?.LogWarning("No dependencies yet for Movement");
        }

        //todo add other dependencies
        
        return expanded.ToList();
    }
}