using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Access;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Policies;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Entity;

public sealed record MovementCategory : IAudited, IDescribed, IOwnedData, IVisibilityScoped
{
    public MovementCategoryId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public string NameKey { get; private set; } = string.Empty;

    public BaseMovementCategory BaseMovementCategory { get; private set; }

    public Description Description { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    public OwnershipInfo Ownership { get; private set; } = null!;
    
    public VisibilityInfo Visibility { get; private set; } = null!;

    private MovementCategory()
    {
        // EF Core
    }

    private MovementCategory(
        MovementCategoryId id,
        string name,
        BaseMovementCategory baseMovementCategory,
        Description description,
        AuditInfo audit,
        OwnershipInfo ownership,
        VisibilityInfo visibility)
    {
        DomainGuard.NotEmptyId(
            id.Value,
            nameof(id));

        DomainGuard.NotEmptyName(
            name,
            nameof(name));

        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentNullException.ThrowIfNull(ownership);
        ArgumentNullException.ThrowIfNull(visibility);
        
        DataAccessPolicy.ValidateOwnership(
            ownership.Origin,
            ownership.OwnerUserId);

        DataAccessPolicy.ValidateVisibility(
            ownership.Origin,
            visibility.Scope);

        Id = id;
        SetName(name);
        BaseMovementCategory = baseMovementCategory;
        Description = description;
        Audit = audit;
        Ownership = ownership;
        Visibility =  visibility;
    }

    public static MovementCategory NewBuiltIn(
        string name,
        string? description,
        BaseMovementCategory baseMovementCategory,
        User? createdBy = null,
        VisibilityScope? visibilityScope = null)
    {
        var ownership = OwnershipInfo.BuiltIn();

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            Description.New(description),
            AuditInfo.New(createdBy),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope)
        );
    }

    public static MovementCategory NewBuiltIn(
        string name,
        BaseMovementCategory baseMovementCategory,
        Description description,
        User? createdBy = null,
        VisibilityScope? visibilityScope = null)
    {
        var ownership = OwnershipInfo.BuiltIn();

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(createdBy),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope)
        );
    }

    public static MovementCategory NewBuiltInWithId(
        MovementCategoryId id,
        string name,
        BaseMovementCategory baseMovementCategory,
        Description description,
        User? createdBy = null,
        VisibilityScope? visibilityScope = null)
    {
        var ownership = OwnershipInfo.BuiltIn();

        return new MovementCategory(
            id,
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(createdBy),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope)
        );
    }

    public static MovementCategory NewUserCreated(
        string name,
        string? description,
        BaseMovementCategory baseMovementCategory,
        User owner,
        VisibilityScope? visibilityScope = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var ownership = OwnershipInfo.UserCreated(owner);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            Description.New(description),
            AuditInfo.New(owner),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope)
        );
    }

    public static MovementCategory NewUserCreated(
        string name,
        BaseMovementCategory baseMovementCategory,
        Description description,
        User owner,
        VisibilityScope? visibilityScope = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var ownership = OwnershipInfo.UserCreated(owner);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(owner),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope)
        );
    }

    public static MovementCategory NewCoachCreated(
        string name,
        BaseMovementCategory baseMovementCategory,
        Description description,
        User coach,
        VisibilityScope? visibilityScope = null)
    {
        ArgumentNullException.ThrowIfNull(coach);

        var ownership = OwnershipInfo.CoachCreated(coach);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(coach),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope)
        );
    }

    public static MovementCategory NewImported(
        string name,
        BaseMovementCategory baseMovementCategory,
        Description description,
        User owner,
        VisibilityScope? visibilityScope = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var ownership = OwnershipInfo.Imported(owner);

        return new MovementCategory(
            MovementCategoryId.New(),
            name,
            baseMovementCategory,
            description,
            AuditInfo.New(owner),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope)
        );
    }

    public bool UpdateVisibility(
        VisibilityInfo visibility,
        User? changedBy = null)
    {
        ArgumentNullException.ThrowIfNull(visibility);

        if (Visibility == visibility)
        {
            return false;
        }

        DataAccessPolicy.ValidateVisibility(
            Ownership.Origin,
            visibility.Scope);

        Visibility = visibility;
        MarkUpdated(changedBy);

        return true;
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
        BaseMovementCategory baseMovementCategory,
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