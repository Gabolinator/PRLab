namespace PRLab.Application.Models.DB.Seeding;

public sealed record SeedItem<TEntity>(
    string Key,
    TEntity Entity,
    SeedAction Action = SeedAction.CreateIfMissing);

public enum SeedAction
{
    CreateIfMissing,
    CreateOrUpdate,
    Ignore
}