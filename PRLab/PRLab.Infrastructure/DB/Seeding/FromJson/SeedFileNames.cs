using PRLab.Domain;

namespace PRLab.Infrastructure.DB.Seeding.FromJson;

public static class SeedFileNames
{
    public static readonly Dictionary<DomainEnum.EntityType, string> FileNamePerEntity =
        new()
        {
            [DomainEnum.EntityType.Equipment] = "equipment.seed.json",
            [DomainEnum.EntityType.Muscle] = "muscles.seed.json",
            [DomainEnum.EntityType.MovementCategory] = "movement-categories.seed.json",
            [DomainEnum.EntityType.Movement] = "movements.seed.json",
        };

    public static string GetSeedFileNameForEntity(Domain.DomainEnum.EntityType entity)
    {
        return FileNamePerEntity.TryGetValue(entity, out var fileName)
            ? fileName
            : throw new ArgumentOutOfRangeException(
                nameof(entity),
                entity,
                $"No seed file name configured for entity type '{entity}'.");
    }

}