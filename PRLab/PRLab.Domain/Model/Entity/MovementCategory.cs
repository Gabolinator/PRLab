using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Entity;

public sealed record MovementCategory : IAudited, IDescribed
{
    public MovementCategoryId Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DomainEnum.BaseMovementCategory BaseMovementCategory { get; init; }

    public Description Description { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

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
    
    public void ChangeDescription(
        string? content,
        string languageCode = "en",
        User? changedBy = null)
    {
        Description = Description.ChangeContent(content, languageCode);
        MarkUpdated(changedBy);
    }

    public void RemoveDescription(
        string languageCode = "en",
        User? changedBy = null)
    {
        Description = Description.RemoveContent(languageCode);
        MarkUpdated(changedBy);
    }
    
    void IAudited.MarkUpdated(User? changedBy)
    {
        MarkUpdated(changedBy);
    }

    void IAudited.MarkDeleted(User? deletedBy)
    {
        MarkDeleted(deletedBy);
    }
    
    private void MarkUpdated(User? changedBy = null)
    {
        Audit = Audit.MarkUpdated(changedBy);
    }

    private void MarkDeleted(User? deletedBy = null)
    {
        Audit = Audit.MarkDeleted(deletedBy);
    }
    
}