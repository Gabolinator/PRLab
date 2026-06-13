using PRLab.Domain;
using PRLab.Domain.Value.Enum.System;

namespace PRLab.Application.Interface.DB.Seeding;


public sealed record SeedResult(
    EntityType EntityType,
    string Name,
    string Version,
    bool Ran,
    IReadOnlyList<SeedChange> Changes)
{
    public bool Changed => Changes.Count > 0;

    public int ChangeCount => Changes.Count;

    public static SeedResult Skipped(
        EntityType entityType,
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
        EntityType entityType,
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