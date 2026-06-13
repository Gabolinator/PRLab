using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;
using PRLab.Domain.Value.Ownership;
using PRLab.Domain.Value.Update;

namespace PRLab.Domain.Model.Entity;

public sealed record MovementCategory : IAudited, IDescribed, IOwnedData
{
    public MovementCategoryId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public string NameKey { get; private set; } = string.Empty;

    public DomainEnum.BaseMovementCategory BaseMovementCategory { get; private set; }

    public Description Description { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    public OwnershipInfo Ownership { get; private set; } = null!;

    private MovementCategory()
    {
        // EF Core
    }

    private MovementCategory(
        MovementCategoryId id,
        string name,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        Description description,
        AuditInfo audit,
        OwnershipInfo ownership)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement category id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentNullException.ThrowIfNull(ownership);

        Id = id;
        SetName(name);
        BaseMovementCategory = baseMovementCategory;
        Description = description;
        Audit = audit;
        Ownership = ownership;
    }

    public static MovementCategory NewBuiltIn(
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
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
    }

    public static MovementCategory NewBuiltIn(
        string name,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        Description description,
        User? createdBy = null)
    {
        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
    }

    public static MovementCategory NewBuiltInWithId(
        MovementCategoryId id,
        string name,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        Description description,
        User? createdBy = null)
    {
        return new MovementCategory(
            id,
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
    }

    public static MovementCategory NewUserCreated(
        string name,
        string? description,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            Description.New(description),
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner)
        );
    }

    public static MovementCategory NewUserCreated(
        string name,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        Description description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner)
        );
    }

    public static MovementCategory NewCoachCreated(
        string name,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        Description description,
        User coach)
    {
        ArgumentNullException.ThrowIfNull(coach);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(coach),
            OwnershipInfo.CoachCreated(coach)
        );
    }

    public static MovementCategory NewImported(
        string name,
        DomainEnum.BaseMovementCategory baseMovementCategory,
        Description description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.Imported(owner)
        );
    }

    public bool Update(MovementCategoryUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var hasChanged = false;

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            hasChanged = TrySetName(update.Name) || hasChanged;
        }

        if (update.BaseMovementCategory.HasValue &&
            BaseMovementCategory != update.BaseMovementCategory.Value)
        {
            BaseMovementCategory = update.BaseMovementCategory.Value;
            hasChanged = true;
        }

        if (update.Description is not null)
        {
            Description = Description.ChangeContent(
                update.Description.Content,
                update.Description.Language
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

    public void ChangeBaseMovementCategory(
        DomainEnum.BaseMovementCategory baseMovementCategory,
        User? changedBy = null)
    {
        if (BaseMovementCategory == baseMovementCategory)
        {
            return;
        }

        BaseMovementCategory = baseMovementCategory;
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