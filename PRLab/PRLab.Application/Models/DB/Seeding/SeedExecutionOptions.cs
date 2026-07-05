using PRLab.Application.Models.DB.Seeding;

public sealed record SeedExecutionOptions
{
    public static SeedExecutionOptions Default => new();

    public static SeedExecutionOptions FullReseed => new()
    {
        ActionOverride = SeedAction.CreateOrUpdate,
        IgnoreSeedHistory = true,
        IgnoreTopLevelIds = true,
        IgnoreReferenceIds = true
    };

    public SeedAction? ActionOverride { get; init; }

    public bool IgnoreSeedHistory { get; init; }

    public bool IgnoreTopLevelIds { get; init; }

    public bool IgnoreReferenceIds { get; init; }

    public SeedAction ResolveAction(SeedAction action)
    {
        return ActionOverride ?? action;
    }
}