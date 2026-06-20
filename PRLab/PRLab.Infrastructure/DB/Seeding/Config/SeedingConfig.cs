using PRLab.Application.Interface.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.Config;

public sealed class SeedingConfig(SeedingOptions options) : ISeedingConfig
{
    public string SeedFileDirectory
    {
        get
        {
            var seedFileDirectory = options.SeedFileDirectory;

            if (string.IsNullOrWhiteSpace(seedFileDirectory))
            {
                throw new InvalidOperationException("Seeding:SeedFileDirectory is not configured.");
            }

            return seedFileDirectory;
        }
    }

    public SeedingSource Source => options.Source;
}