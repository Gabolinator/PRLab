using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Access;
using PRLab.Domain.Model.Value.Enum.Equipment;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Policies;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Entity;

public sealed record Equipment : IAudited, IDescribed, IOwnedData, IVisibilityScoped
{
    public EquipmentId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public string NameKey { get; private set; } = string.Empty;

    private List<EquipmentConfigurationType> supportedConfigurationTypes = [];

    public IReadOnlyCollection<EquipmentConfigurationType> SupportedConfigurationTypes =>
        supportedConfigurationTypes.AsReadOnly();

    private readonly List<EquipmentAlias> aliases = [];

    public IReadOnlyCollection<EquipmentAlias> Aliases => aliases.AsReadOnly();

    public void AddAlias(string value) => AddAlias(Alias.New(value));

    public void AddAlias(Alias alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        if (aliases.Any(relation => relation.AliasValueKey == alias.ValueKey))
        {
            throw new InvalidOperationException($"Alias '{alias.Value}' already exists for '{Name}'.");
        }

        aliases.Add(EquipmentAlias.New(Id, alias));
    }

    public bool RemoveAlias(string value)
    {
        var valueKey = Alias.GetValueKey(value);
        var relation = aliases.SingleOrDefault(link => link.AliasValueKey == valueKey);

        return relation is not null && aliases.Remove(relation);
    }

    public Description Description { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    public OwnershipInfo Ownership { get; private set; } = null!;

    public VisibilityInfo Visibility { get; private set; } = null!;

    private Equipment()
    {
        // EF Core
    }

    private Equipment(
        EquipmentId id,
        string name,
        Description description,
        AuditInfo audit,
        OwnershipInfo ownership,
        VisibilityInfo visibility,
        IReadOnlyCollection<EquipmentConfigurationType>? supportedConfigurationTypes)
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
        Description = description;
        Audit = audit;
        Ownership = ownership;
        Visibility = visibility;
        this.supportedConfigurationTypes = NormalizeConfigurationTypes(supportedConfigurationTypes ?? []);
    }

    public static Equipment NewBuiltIn(
        string name,
        string? description,
        User? createdBy = null,
        VisibilityScope? visibilityScope = null,
        IReadOnlyCollection<EquipmentConfigurationType>? supportedConfigurationTypes = null) =>
        NewBuiltIn(
            name,
            Description.New(description),
            createdBy,
            visibilityScope,
            supportedConfigurationTypes);

    public static Equipment NewBuiltIn(
        string name,
        Description description,
        User? createdBy = null,
        VisibilityScope? visibilityScope = null,
        IReadOnlyCollection<EquipmentConfigurationType>? supportedConfigurationTypes = null) =>
        NewBuiltInWithId(
            EquipmentId.New(),
            name,
            description,
            createdBy,
            visibilityScope,
            supportedConfigurationTypes);

    public static Equipment NewBuiltInWithId(
        EquipmentId id,
        string name,
        Description description,
        User? createdBy = null,
        VisibilityScope? visibilityScope = null,
        IReadOnlyCollection<EquipmentConfigurationType>? supportedConfigurationTypes = null)
    {
        var ownership = OwnershipInfo.BuiltIn();

        return new Equipment(
            id,
            name,
            description,
            AuditInfo.New(createdBy),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope),
            supportedConfigurationTypes);
    }

    public static Equipment NewUserCreated(
        string name,
        string? description,
        User owner,
        VisibilityScope? visibilityScope = null,
        IReadOnlyCollection<EquipmentConfigurationType>? supportedConfigurationTypes = null) =>
        NewUserCreated(
            name,
            Description.New(description),
            owner,
            visibilityScope,
            supportedConfigurationTypes);

    public static Equipment NewUserCreated(
        string name,
        Description description,
        User owner,
        VisibilityScope? visibilityScope = null,
        IReadOnlyCollection<EquipmentConfigurationType>? supportedConfigurationTypes = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var ownership = OwnershipInfo.UserCreated(owner);
        
        return new Equipment(
            EquipmentId.New(),
            name,
            description,
            AuditInfo.New(owner),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope),
            supportedConfigurationTypes);
    }

    public static Equipment NewCoachCreated(
        string name,
        Description description,
        User coach,
        VisibilityScope? visibilityScope = null,
        IReadOnlyCollection<EquipmentConfigurationType>? supportedConfigurationTypes = null)
    {
        ArgumentNullException.ThrowIfNull(coach);

        var ownership = OwnershipInfo.CoachCreated(coach);
        
        return new Equipment(
            EquipmentId.New(),
            name,
            description,
            AuditInfo.New(coach),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope),
            supportedConfigurationTypes);
    }

    public static Equipment NewImported(
        string name,
        Description description,
        User owner,
        VisibilityScope? visibilityScope = null,
        IReadOnlyCollection<EquipmentConfigurationType>? supportedConfigurationTypes = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var ownership = OwnershipInfo.Imported(owner);
        
        return new Equipment(
            EquipmentId.New(),
            name,
            description,
            AuditInfo.New(owner),
            ownership,
            VisibilityInfo.FromPreferenceOrDefault(ownership, visibilityScope),
            supportedConfigurationTypes);
    }

    public bool Update(EquipmentUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var configurationTypes = update.SupportedConfigurationTypes is { } updatedConfigurationTypes
            ? NormalizeConfigurationTypes(updatedConfigurationTypes)
            : null;
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

        if (configurationTypes is not null && !supportedConfigurationTypes.SequenceEqual(configurationTypes))
        {
            supportedConfigurationTypes = configurationTypes;
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

    public bool UpdateVisibility(
        VisibilityInfo visibility,
        User? changedBy = null)
    {
        ArgumentNullException.ThrowIfNull(visibility);

        if (Visibility == visibility)
        {
            return false;
        }

        VisibilityPolicy.ValidateVisibilityForOwnership(
            Ownership,
            visibility);

        Visibility = visibility;
        MarkUpdated(changedBy);

        return true;
    }
    
    private static List<EquipmentConfigurationType> NormalizeConfigurationTypes(
        IReadOnlyCollection<EquipmentConfigurationType> configurationTypes)
    {
        if (configurationTypes.Any(type => !Enum.IsDefined(type)))
        {
            throw new ArgumentOutOfRangeException(nameof(configurationTypes), "Unknown equipment configuration type.");
        }

        return configurationTypes
            .Distinct()
            .Order()
            .ToList();
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
