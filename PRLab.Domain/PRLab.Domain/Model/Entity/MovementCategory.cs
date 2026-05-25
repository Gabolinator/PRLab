using PRLab;
using PRLab.Model.Entity;
using PRLab.Model.Interface;
using PRLab.Utilities;
using PRLab.Value.Identifier;

public sealed record MovementCategory : IAudited
{
    public MovementCategoryId Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DomainEnum.BaseMovementCategory BaseMovementCategory { get; init; }

    public Description Description { get; init; } = null!;

    public AuditInfo Audit { get; init; } = null!;

    private MovementCategory()
    {
        // EF Core
    }

    private MovementCategory(
        MovementCategoryId id,
        string name,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        Description description,
        AuditInfo audit)
    {
        Id = id;
        Name = FormatingUtilities.NormalizeName(name);
        BaseMovementCategory = baseMovementCategory;
        Description = description;
        Audit = audit;
    }

    public static MovementCategory New(
        string name,
        string? description,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        User? createdBy = null)
    {
        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            Description.New(description),
            AuditInfo.New(createdBy)
        );
    }
    
    public MovementCategory ChangeDescription(string? content, User? changedBy = null)
    {
        return this with
        {
            Description = Description.ChangeContent(content),
            Audit = Audit.MarkUpdated(changedBy)
        };
    }

    public MovementCategory RemoveDescription(User? changedBy = null)
    {
        return ChangeDescription(null, changedBy);
    }
    
}