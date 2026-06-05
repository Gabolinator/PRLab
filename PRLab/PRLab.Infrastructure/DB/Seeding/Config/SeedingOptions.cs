namespace PRLab.Infrastructure.DB.Seeding.Config;

public sealed class SeedingOptions
{
    public string SeedFileDirectory { get; init; } = string.Empty;
    public bool SeedFromJsonFile { get; set; } = true;
}