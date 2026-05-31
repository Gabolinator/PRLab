using PRLab.Application.Interface.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding;

public sealed class DevelopmentDataSeeder(
    IEnumerable<IEntitySeeder> entitySeeders
) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        foreach (var entitySeeder in entitySeeders.OrderBy(entitySeeder => entitySeeder.Order))
        {
            await entitySeeder.SeedAsync(ct);
        }
    }
}