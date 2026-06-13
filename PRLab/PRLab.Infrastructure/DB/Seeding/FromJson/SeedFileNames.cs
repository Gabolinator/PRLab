using PRLab.Domain;
using PRLab.Domain.Value.Enum.System;

namespace PRLab.Infrastructure.DB.Seeding.FromJson;

public static class SeedFileNames
{
    public static readonly Dictionary<EntityType, string> FileNamePerEntity =
        new()
        {
            [EntityType.Equipment] = "equipment.seed.json",
            [EntityType.Muscle] = "muscles.seed.json",
            [EntityType.MovementCategory] = "movement-categories.seed.json",
            [EntityType.Movement] = "movements.seed.json",
        };

    public static string GetSeedFileNameForEntity(EntityType entity)
    {
        return FileNamePerEntity.TryGetValue(entity, out var fileName)
            ? fileName
            : throw new ArgumentOutOfRangeException(
                nameof(entity),
                entity,
                $"No seed file name configured for entity type '{entity}'.");
    }

}