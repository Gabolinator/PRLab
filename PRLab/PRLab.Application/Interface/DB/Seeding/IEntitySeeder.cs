using PRLab.Domain;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IEntitySeeder
{
    int Order { get; }

    string Name { get; }

    string Version { get; }
    DomainEnum.EntityType EntityType { get;}

    Task<SeedResult> SeedAsync(CancellationToken ct = default);
    
    Task<bool> AlreadySeededAsync(CancellationToken ct = default);
}