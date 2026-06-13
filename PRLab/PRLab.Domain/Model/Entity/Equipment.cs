using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;
using PRLab.Domain.Value.Ownership;
using PRLab.Domain.Value.Update;

namespace PRLab.Domain.Model.Entity;

public sealed record Equipment : IAudited, IDescribed, IOwnedData
{
    public EquipmentId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public string NameKey { get; private set; } = string.Empty;

    public Description Description { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    public OwnershipInfo Ownership { get; private set; } = null!;

    private Equipment()
    {
        // EF Core
    }

    private Equipment(
        EquipmentId id,
        string name,
        Description description,
        AuditInfo audit,
        OwnershipInfo ownership)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Equipment id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentNullException.ThrowIfNull(ownership);

        Id = id;
        SetName(name);
        Description = description;
        Audit = audit;
        Ownership = ownership;
    }

    public static Equipment NewBuiltIn(
        string name,
        string? description,
        User? createdBy = null)
    {
        return new Equipment(
            EquipmentId.New(),
            name,
            Description.New(description),
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
    }

    public static Equipment NewBuiltIn(
        string name,
        Description description,
        User? createdBy = null)
    {
        return new Equipment(
            EquipmentId.New(),
            name,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
    }

    public static Equipment NewBuiltInWithId(
        EquipmentId id,
        string name,
        Description description,
        User? createdBy = null)
    {
        return new Equipment(
            id,
            name,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
    }

    public static Equipment NewUserCreated(
        string name,
        string? description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Equipment(
            EquipmentId.New(),
            name,
            Description.New(description),
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner)
        );
    }

    public static Equipment NewUserCreated(
        string name,
        Description description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Equipment(
            EquipmentId.New(),
            name,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner)
        );
    }

    public static Equipment NewCoachCreated(
        string name,
        Description description,
        User coach)
    {
        ArgumentNullException.ThrowIfNull(coach);

        return new Equipment(
            EquipmentId.New(),
            name,
            description,
            AuditInfo.New(coach),
            OwnershipInfo.CoachCreated(coach)
        );
    }

    public static Equipment NewImported(
        string name,
        Description description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Equipment(
            EquipmentId.New(),
            name,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.Imported(owner)
        );
    }

    public bool Update(EquipmentUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var hasChanged = false;

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            hasChanged = TrySetName(update.Name) || hasChanged;
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

        return hasChanged;
    }

    public void Rename(
        string name,
        User? changedBy = null)
    {
        if (TrySetName(name))
        {
            MarkUpdated(changedBy);
        }
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

    private void SetName(string name)
    {
        Name = FormatingUtilities.NormalizeName(name);
        NameKey = FormatingUtilities.NormalizeNameKey(name);
    }

    private bool TrySetName(string name)
    {
        var normalizedName = FormatingUtilities.NormalizeName(name);
        var normalizedNameKey = FormatingUtilities.NormalizeNameKey(name);

        if (Name == normalizedName && NameKey == normalizedNameKey)
        {
            return false;
        }

        Name = normalizedName;
        NameKey = normalizedNameKey;

        return true;
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