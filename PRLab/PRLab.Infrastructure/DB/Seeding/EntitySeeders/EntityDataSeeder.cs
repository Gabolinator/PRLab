using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class EntityDataSeeder(
    IEnumerable<IEntitySeeder> entitySeeders,
    IAppLogger logger
) : IDataSeeder
{
    private readonly IReadOnlyList<IEntitySeeder> entitySeeders = entitySeeders.ToList();

    public IReadOnlySet<EntityType> EntitySeederTypes =>
        entitySeeders
            .Select(seeder => seeder.EntityType)
            .ToHashSet();
    
    public IReadOnlySet<EntityType> BaseEntitySeederTypes =>
        entitySeeders
            .Select(seeder => seeder.EntityType)
            .Where(type => type.IsBaseType())
            .ToHashSet();

    public async Task<IReadOnlyList<SeedResult>> SeedAsync(
        IReadOnlyCollection<EntityType>? entities = null,
        CancellationToken ct = default)
    {
        var entitiesToSeed = entities is null || entities.Count == 0
            ? EntitySeederTypes
            : entities.ToHashSet();

        ValidateRequestedEntities(entitiesToSeed);

        var results = new List<SeedResult>();

        foreach (var entitySeeder in entitySeeders
                     .Where(entitySeeder => entitiesToSeed.Contains(entitySeeder.EntityType))
                     .OrderBy(entitySeeder => entitySeeder.Order))
        {
            var result = await entitySeeder.SeedAsync(ct);

            logger.Log($"Seeded {entitySeeder.EntityType} - Changes: {result.ChangeCount}");

            results.Add(result);
        }

        return results;
    }

    private void ValidateRequestedEntities(
        IReadOnlySet<EntityType> requestedEntities)
    {
        var unsupportedEntities = requestedEntities
            .Where(entityType => !EntitySeederTypes.Contains(entityType))
            .ToList();

        if (unsupportedEntities.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"No entity seeder registered for: {string.Join(", ", unsupportedEntities)}.");
    }
}