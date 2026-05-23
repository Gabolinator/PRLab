using PRLab.Model.Interface;
using PRLab.Value.Identifier;

namespace PRLab.Model.Entity;

public sealed record Equipment : IAudited
{
    public EquipmentId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Description Description { get; init; } = null!;
    public AuditInfo Audit { get; init; } = null!;

    private Equipment()
    {
        // EF Core
    }

    private Equipment(
        EquipmentId id,
        string name,
        Description description,
        AuditInfo audit)
    {
        Id = id;
        Name = name;
        Description = description;
        Audit = audit;
    }

    public static Equipment New(string name, string? description, User? createdBy = null)
    {
        return new Equipment(
            EquipmentId.New(),
            name,
            Description.New(description),
            AuditInfo.New(createdBy)
        );
    }

    public Equipment ChangeDescription(string? content, User? changedBy = null)
    {
        return this with
        {
            Description = Description.ChangeContent(content),
            Audit = Audit.MarkUpdated(changedBy)
        };
    }

    public Equipment RemoveDescription(User? changedBy = null)
    {
        return ChangeDescription(null, changedBy);
    }
}