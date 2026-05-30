using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;
using PRLab.Domain.Value.Update;

namespace PRLab.Domain.Model.Entity;

public sealed record MovementCategory : IAudited, IDescribed
{
    public MovementCategoryId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public string NameKey { get; private set; } = string.Empty;

    public DomainEnum.BaseMovementCategory BaseMovementCategory { get; private set; }

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
        SetName(name);
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
    
    public static MovementCategory New(
        string name,
        Description description,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        User? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(description);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(createdBy));
    }
    
    private void SetName(string name)
    {
        Name = FormatingUtilities.NormalizeName(name);
        NameKey = FormatingUtilities.NormalizeNameKey(name);
    }
    
    public void Update(MovementCategoryUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var hasChanged = false;

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            SetName(update.Name);
            hasChanged = true;
        }

        if (update.BaseMovementCategory.HasValue)
        {
           BaseMovementCategory = update.BaseMovementCategory.Value;
            hasChanged = true;
        }

        if (update.Description != null)
        {
           Description.ChangeContent(
               update.Description.Content,
               update.Description.Language);
            
           hasChanged = true;
        }
        
        if (hasChanged)
        {
            MarkUpdated(update.UpdatedBy);
        }
    }
    
    private void Rename(string name, User? updatedBy = null)
    { 
       SetName(name);
       MarkUpdated(updatedBy);
    }
    
    public void ChangeDescription(
        string? content,
        LocalizationHelper.Language? languageCode,
        User? changedBy = null)
    {
        Description = Description.ChangeContent(content, languageCode);
        MarkUpdated(changedBy);
    }

    public void RemoveDescription(
        LocalizationHelper.Language? languageCode,
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