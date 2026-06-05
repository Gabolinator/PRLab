using PRLab.Domain;

namespace PRLab.Application.Interface.DB.Seeding;


public sealed record SeedResult(
    DomainEnum.EntityType EntityType,
    string Name,
    string Version,
    bool Ran,
    IReadOnlyList<SeedChange> Changes)
{
    public bool Changed => Changes.Count > 0;

    public int ChangeCount => Changes.Count;

    public static SeedResult Skipped(
        DomainEnum.EntityType entityType,
        string name,
        string version)
    {
        return new SeedResult(
            entityType,
            name,
            version,
            Ran: false,
            Changes: []);
    }

    public static SeedResult FromChanges(
        DomainEnum.EntityType entityType,
        string name,
        string version,
        IReadOnlyList<SeedChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        return new SeedResult(
            entityType,
            name,
            version,
            Ran: true,
            Changes: changes);
    }
}

public sealed record SeedChange(
    string Key,
    SeedChangeType Type);
    
public enum SeedChangeType
{
    Created,
    Updated,
}