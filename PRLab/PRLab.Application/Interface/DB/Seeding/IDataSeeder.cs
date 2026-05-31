namespace PRLab.Application.Interface.DB.Seeding;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}