namespace PRLab.Application.Interface.DB.Seeding;

public interface IEntitySeeder
{
    int Order { get; }

    string Name { get; }

    string Version { get; }

    Task SeedAsync(CancellationToken ct = default);
}