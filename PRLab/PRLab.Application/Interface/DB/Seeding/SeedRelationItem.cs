namespace PRLab.Application.Interface.DB.Seeding;

public sealed record SeedRelationItem(
    string SourceKey,
    string TargetKey,
    SeedAction Action = SeedAction.CreateIfMissing);
    
public sealed record SeedRelationItem<TId>(
    TId SourceId,
    TId TargetId,
    SeedAction Action = SeedAction.CreateIfMissing);