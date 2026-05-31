namespace PRLab.Application.Interface.DB.Seeding;

public sealed record SeedRelationItem(
    string SourceKey,
    string TargetKey,
    SeedAction Action = SeedAction.CreateIfMissing);