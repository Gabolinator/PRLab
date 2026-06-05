using Microsoft.Extensions.Options;
using PRLab.Application.Interface.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.Config;

public sealed class SeedingConfig(IOptions<SeedingOptions> options) : ISeedingConfig
{
    public string SeedFileDirectory
    {
        get
        {
            var seedFileDirectory = options.Value.SeedFileDirectory;

            if (string.IsNullOrWhiteSpace(seedFileDirectory))
            {
                throw new InvalidOperationException("Seeding:SeedFileDirectory is not configured.");
            }

            return seedFileDirectory;
        }
    }
    
    public bool SeedFromFile
    {
        get
        {
            var seedFromFile = options.Value.SeedFromJsonFile;
            return seedFromFile;
        }
    }
}