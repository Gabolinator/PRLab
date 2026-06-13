using PRLab.Application.Interface.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.Config;

public sealed class SeedingOptions
{
    public string SeedFileDirectory { get; init; } = string.Empty;
    
    public SeedingSource Source { get; init; } =  SeedingSource.JsonFiles;
}