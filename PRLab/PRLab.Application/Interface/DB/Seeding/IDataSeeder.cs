using PRLab.Domain;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IDataSeeder
{
    public IReadOnlySet<DomainEnum.EntityType> EntitySeederTypes { get; }
    
    Task<IReadOnlyList<SeedResult>> SeedAsync(
        IReadOnlyCollection<DomainEnum.EntityType>? entities = null,
        CancellationToken ct = default);
}