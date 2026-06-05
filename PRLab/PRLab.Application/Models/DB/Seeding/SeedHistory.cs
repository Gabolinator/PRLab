namespace PRLab.Application.Models.DB.Seeding;

public sealed record SeedHistory
{
    public SeedHistoryId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public string Version { get; private set; } = string.Empty;

    public DateTimeOffset AppliedAtUtc { get; private set; }

    private SeedHistory()
    {
        // EF Core
    }

    private SeedHistory(
        SeedHistoryId id,
        string name,
        string version,
        DateTimeOffset appliedAtUtc)
    {
        Id = id;
        Name = name;
        Version = version;
        AppliedAtUtc = appliedAtUtc;
    }

    public static SeedHistory New(
        string name,
        string version,
        DateTimeOffset appliedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Seed history name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Seed history version cannot be empty.", nameof(version));
        }

        return new SeedHistory(
            SeedHistoryId.New(),
            name.Trim(),
            version.Trim(),
            appliedAtUtc);
    }
}