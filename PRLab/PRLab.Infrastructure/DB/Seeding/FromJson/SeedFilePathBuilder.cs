namespace PRLab.Infrastructure.DB.Seeding.FromJson;

public static class SeedFilePathBuilder
{
    public static string Build(string seedFileDirectory, string seedFileName)
    {
        if (string.IsNullOrWhiteSpace(seedFileDirectory))
        {
            throw new ArgumentException("Seed file directory cannot be empty.", nameof(seedFileDirectory));
        }

        if (string.IsNullOrWhiteSpace(seedFileName))
        {
            throw new ArgumentException("Seed file name cannot be empty.", nameof(seedFileName));
        }

        return Path.Combine(seedFileDirectory, seedFileName);
    }
}