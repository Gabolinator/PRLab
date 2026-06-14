using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IDataSeeder
{
    public IReadOnlySet<EntityType> EntitySeederTypes { get; }
    public IReadOnlySet<EntityType> BaseEntitySeederTypes { get; }
    
    Task<IReadOnlyList<SeedResult>> SeedAsync(
        IReadOnlyCollection<EntityType>? entities = null,
        CancellationToken ct = default);
}