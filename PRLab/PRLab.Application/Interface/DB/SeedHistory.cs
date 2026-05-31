namespace PRLab.Application.Interface.DB;

public sealed record SeedHistory
{
    public string Name { get; init; } = string.Empty;

    public DateTimeOffset AppliedAtUtc { get; init; }

    private SeedHistory()
    {
        // EF Core
    }

    private SeedHistory(string name, DateTimeOffset appliedAtUtc)
    {
        Name = name;
        AppliedAtUtc = appliedAtUtc;
    }

    public static SeedHistory New(string name, DateTimeOffset appliedAtUtc)
    {
        return new SeedHistory(name, appliedAtUtc);
    }
}