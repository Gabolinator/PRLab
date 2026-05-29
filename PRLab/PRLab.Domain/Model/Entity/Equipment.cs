using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;
using PRLab.Domain.Value.Update;

namespace PRLab.Domain.Model.Entity;

public sealed record Equipment : IAudited, IDescribed
{
    public EquipmentId Id { get; init; }
    public string Name { get; private set; } = string.Empty;
    public Description Description { get; private set; } = null!;
    public AuditInfo Audit { get; private set; } = null!;

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
        Name = FormatingUtilities.NormalizeName(name);
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

    public static Equipment New(
        string name,
        Description description,
        User? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(description);

        return new Equipment(
            EquipmentId.New(),
            name,
            description,
            AuditInfo.New(createdBy)
        );
    }
    
    public void Update(EquipmentUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var hasChanged = false;

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            Name = FormatingUtilities.NormalizeName(update.Name);
            hasChanged = true;
        }

        if (update.DescriptionUpdate is not null)
        {
            Description = Description.ChangeContent(
                update.DescriptionUpdate.Content,
                update.DescriptionUpdate.Language
            );

            hasChanged = true;
        }

        if (hasChanged)
        {
            MarkUpdated(update.UpdatedBy);
        }
    }
    
    public void ChangeName(
        string name,
        User? changedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name cannot be empty.", nameof(name));
        }

        Name = FormatingUtilities.NormalizeName(name);
        MarkUpdated(changedBy);
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