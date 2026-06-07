using PRLab.Domain;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IDataSeeder
{
    public IReadOnlySet<DomainEnum.EntityType> EntitySeederTypes { get; }
    public IReadOnlySet<DomainEnum.EntityType> BaseEntitySeederTypes { get; }
    
    Task<IReadOnlyList<SeedResult>> SeedAsync(
        IReadOnlyCollection<DomainEnum.EntityType>? entities = null,
        CancellationToken ct = default);
}