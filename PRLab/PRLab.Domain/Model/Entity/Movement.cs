using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Entity;

public sealed record Movement : IAudited, IDescribed, IOwnedData
{
    public MovementId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public string NameKey { get; private set; } = string.Empty;

    public MovementCategoryId MovementCategoryId { get; private set; }

    public MovementCategory MovementCategory { get; private set; } = null!;

    public MovementLaterality Laterality { get; private set; }
    
    public MovementId? VariantOfId { get; private set; }

    public Movement? VariantOf { get; private set; }

    private readonly List<Movement> variants = [];

    public IReadOnlyCollection<Movement> Variants => variants;
    
    public MovementPattern? PrimaryPattern { get; private set; }

    private readonly List<MovementPatternTag> patterns = [];

    public IReadOnlyCollection<MovementPatternTag> Patterns => patterns;

    private HashSet<MovementPattern> PatternValues => Patterns
        .Select(patternTag => patternTag.Pattern)
        .ToHashSet();

    private readonly List<MovementMuscle> muscles = [];

    public IReadOnlyCollection<MovementMuscle> Muscles => muscles;

    private readonly List<MovementEquipmentRequirement> equipmentRequirements = [];
    
    public IReadOnlyCollection<MovementEquipmentRequirement> EquipmentRequirements => equipmentRequirements;

    public WorkTargetType DefaultWorkTargetType { get; private set; }

    private readonly List<MovementAllowedWorkTarget> allowedWorkTargets = [];

    public IReadOnlyCollection<MovementAllowedWorkTarget> AllowedWorkTargets => allowedWorkTargets;

    private HashSet<WorkTargetType> AllowedWorkTargetTypes => AllowedWorkTargets
        .Select(allowedWorkTarget => allowedWorkTarget.TargetType)
        .ToHashSet();
    
    public Description Description { get; private set; } = null!;
    
    public OwnershipInfo Ownership { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    private HashSet<MovementId> VariantIDs => Variants
        .Select(variant => variant.Id)
        .ToHashSet();

    private HashSet<MuscleId> MuscleIDs => Muscles
        .Select(movementMuscle => movementMuscle.MuscleId)
        .ToHashSet();

    private HashSet<MuscleId> PrimaryMuscleIDs => Muscles
        .Where(movementMuscle => movementMuscle.Role == MuscleRole.Primary)
        .Select(movementMuscle => movementMuscle.MuscleId)
        .ToHashSet();

    private HashSet<MuscleId> SecondaryMuscleIDs => Muscles
        .Where(movementMuscle => movementMuscle.Role == MuscleRole.Secondary)
        .Select(movementMuscle => movementMuscle.MuscleId)
        .ToHashSet();

    private HashSet<EquipmentId> EquipmentIDs => EquipmentRequirements
        .Select(movementEquipment => movementEquipment.EquipmentId)
        .ToHashSet();

    private Movement()
    {
        // EF Core
    }

    private Movement(
        MovementId id,
        string name,
        MovementCategory movementCategory,
        Description description,
        AuditInfo audit,
        OwnershipInfo ownership,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality = MovementLaterality.Bilateral,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(movementCategory);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentNullException.ThrowIfNull(ownership);

        Id = id;
        SetName(name);
        MovementCategory = movementCategory;
        MovementCategoryId = movementCategory.Id;
        Laterality = laterality;
        Description = description;
        Audit = audit;
        Ownership = ownership;

        SetWorkTargetTypesDuringCreation(
            defaultWorkTargetType,
            allowedWorkTargetTypes);
    }

    private Movement(
        MovementId id,
        string name,
        MovementCategoryId movementCategoryId,
        Description description,
        AuditInfo audit,
        OwnershipInfo ownership,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality = MovementLaterality.Bilateral,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(id));
        }

        if (movementCategoryId.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement category id cannot be empty.", nameof(movementCategoryId));
        }

        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentNullException.ThrowIfNull(ownership);

        Id = id;
        SetName(name);
        MovementCategoryId = movementCategoryId;
        Laterality = laterality;
        Description = description;
        Audit = audit;
        Ownership = ownership;

        SetWorkTargetTypesDuringCreation(
            defaultWorkTargetType,
            allowedWorkTargetTypes);
    }

    public static Movement NewBuiltIn(
        string name,
        MovementCategoryId movementCategoryId,
        string? description,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null,
        User? createdBy = null)
    {
        return new Movement(
            MovementId.New(),
            name,
            movementCategoryId,
            Description.New(description),
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn(),
            defaultWorkTargetType,
            laterality,
            allowedWorkTargetTypes
        );
    }

    public static Movement NewBuiltIn(
        string name,
        MovementCategory movementCategory,
        Description description,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null,
        User? createdBy = null)
    {
        return new Movement(
            MovementId.New(),
            name,
            movementCategory,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn(),
            defaultWorkTargetType,
            laterality,
            allowedWorkTargetTypes
        );
    }

    public static Movement NewBuiltInWithId(
        MovementId id,
        string name,
        MovementCategory movementCategory,
        Description description,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null,
        User? createdBy = null)
    {
        return new Movement(
            id,
            name,
            movementCategory,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn(),
            defaultWorkTargetType,
            laterality,
            allowedWorkTargetTypes
        );
    }

    public static Movement NewUserCreated(
        string name,
        MovementCategoryId movementCategoryId,
        string? description,
        User owner,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Movement(
            MovementId.New(),
            name,
            movementCategoryId,
            Description.New(description),
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner),
            defaultWorkTargetType,
            laterality,
            allowedWorkTargetTypes
        );
    }

    public static Movement NewUserCreated(
        string name,
        MovementCategory movementCategory,
        Description description,
        User owner,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Movement(
            MovementId.New(),
            name,
            movementCategory,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner),
            defaultWorkTargetType,
            laterality,
            allowedWorkTargetTypes
        );
    }

    public static Movement NewCoachCreated(
        string name,
        MovementCategory movementCategory,
        Description description,
        User coach,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality = MovementLaterality.Bilateral,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null)
    {
        ArgumentNullException.ThrowIfNull(coach);

        return new Movement(
            MovementId.New(),
            name,
            movementCategory,
            description,
            AuditInfo.New(coach),
            OwnershipInfo.CoachCreated(coach),
            defaultWorkTargetType,
            laterality,
            allowedWorkTargetTypes
        );
    }

    public static Movement NewImported(
        string name,
        MovementCategory movementCategory,
        Description description,
        User owner,
        WorkTargetType defaultWorkTargetType,
        MovementLaterality laterality = MovementLaterality.Bilateral,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Movement(
            MovementId.New(),
            name,
            movementCategory,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.Imported(owner),
            defaultWorkTargetType,
            laterality,
            allowedWorkTargetTypes
        );
    }
    
    private void SetName(string name)
    {
        Name = FormatingUtilities.NormalizeName(name);
        NameKey = FormatingUtilities.NormalizeNameKey(name);
    }

    public void Rename(string name, User? changedBy = null)
    {
        if (TrySetName(name))
        {
            MarkUpdated(changedBy);
        }
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
    
    public void ChangeLaterality(
        MovementLaterality laterality,
        User? changedBy = null)
    {
        if (Laterality == laterality)
        {
            return;
        }

        Laterality = laterality;
        MarkUpdated(changedBy);
    }

    public void ChangeCategory(MovementCategoryId movementCategoryId, User? changedBy = null)
    {
        MovementCategoryId = movementCategoryId;
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
    
    public void SetDefaultWorkTargetType(
        WorkTargetType targetType,
        User? changedBy = null)
    {
        if (!AllowedWorkTargetTypes.Contains(targetType))
        {
            AddAllowedWorkTargetTypeWithoutAudit(targetType);
        }

        if (DefaultWorkTargetType == targetType)
        {
            return;
        }

        DefaultWorkTargetType = targetType;
        MarkUpdated(changedBy);
    }

    public void AddAllowedWorkTargetType(
        WorkTargetType targetType,
        User? changedBy = null)
    {
        if (AllowedWorkTargetTypes.Contains(targetType))
        {
            return;
        }

        AddAllowedWorkTargetTypeWithoutAudit(targetType);

        MarkUpdated(changedBy);
    }

    public void RemoveAllowedWorkTargetType(
        WorkTargetType targetType,
        User? changedBy = null)
    {
        if (DefaultWorkTargetType == targetType)
        {
            throw new InvalidOperationException(
                "Cannot remove the default work target type from allowed work target types.");
        }

        var allowedWorkTarget = allowedWorkTargets
            .FirstOrDefault(allowedWorkTarget => allowedWorkTarget.TargetType == targetType);

        if (allowedWorkTarget is null)
        {
            return;
        }

        allowedWorkTargets.Remove(allowedWorkTarget);

        MarkUpdated(changedBy);
    }
    
    private void SetWorkTargetTypesDuringCreation(
        WorkTargetType defaultWorkTargetType,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes)
    {
        DefaultWorkTargetType = defaultWorkTargetType;

        var targetTypes = allowedWorkTargetTypes is null || allowedWorkTargetTypes.Count == 0
            ? [defaultWorkTargetType]
            : allowedWorkTargetTypes.ToHashSet();

        targetTypes.Add(defaultWorkTargetType);

        foreach (var targetType in targetTypes)
        {
            AddAllowedWorkTargetTypeWithoutAudit(targetType);
        }
    }

    private void AddAllowedWorkTargetTypeWithoutAudit(WorkTargetType targetType)
    {
        if (AllowedWorkTargetTypes.Contains(targetType))
        {
            return;
        }

        allowedWorkTargets.Add(
            MovementAllowedWorkTarget.New(
                Id,
                targetType));
    }
    
    void IAudited.MarkUpdated(User? changedBy)
    {
        MarkUpdated(changedBy);
    }

    void IAudited.MarkDeleted(User? deletedBy)
    {
        MarkDeleted(deletedBy);
    }

    public void AddPrimaryMuscle(MuscleId muscleId, User? changedBy = null)
    {
        AddMuscle(
            muscleId,
            MuscleRole.Primary,
            changedBy
        );
    }

    public void AddSecondaryMuscle(MuscleId muscleId, User? changedBy = null)
    {
        AddMuscle(
            muscleId,
            MuscleRole.Secondary,
            changedBy
        );
    }

    public void AddMuscle(
        MuscleId muscleId,
        MuscleRole role,
        User? changedBy = null)
    {
        if (MuscleIDs.Contains(muscleId))
        {
            return;
        }

        muscles.Add(
            MovementMuscle.New(
                Id,
                muscleId,
                role
            )
        );

        MarkUpdated(changedBy);
    }

    public void ChangeMuscleRole(
        MuscleId muscleId,
        MuscleRole role,
        User? changedBy = null)
    {
        if (!MuscleIDs.Contains(muscleId))
        {
            return;
        }

        var movementMuscle = muscles
            .FirstOrDefault(movementMuscle => movementMuscle.MuscleId == muscleId);

        if (movementMuscle is null)
        {
            return;
        }

        muscles.Remove(movementMuscle);

        muscles.Add(
            MovementMuscle.New(
                Id,
                muscleId,
                role
            )
        );

        MarkUpdated(changedBy);
    }

    public void RemoveMuscle(MuscleId muscleId, User? changedBy = null)
    {
        if (!MuscleIDs.Contains(muscleId))
        {
            return;
        }

        var movementMuscle = muscles
            .FirstOrDefault(movementMuscle => movementMuscle.MuscleId == muscleId);

        if (movementMuscle is null)
        {
            return;
        }

        muscles.Remove(movementMuscle);

        MarkUpdated(changedBy);
    }

    public void AddRequiredEquipmentOption(
        EquipmentId equipmentId,
        string groupKey,
        User? changedBy = null)
    {
        AddEquipmentRequirement(
            equipmentId,
            groupKey,
            EquipmentRequirementKind.RequiredGroup,
            changedBy);
    }

    public void AddOptionalEquipment(
        EquipmentId equipmentId,
        string groupKey,
        User? changedBy = null)
    {
        AddEquipmentRequirement(
            equipmentId,
            groupKey,
            EquipmentRequirementKind.Optional,
            changedBy);
    }

    private void AddEquipmentRequirement(
        EquipmentId equipmentId,
        string groupKey,
        EquipmentRequirementKind kind,
        User? changedBy = null)
    {
        var normalizedGroupKey = groupKey.Trim().ToLowerInvariant();

        var alreadyExists = equipmentRequirements.Any(requirement =>
            requirement.EquipmentId == equipmentId
            && requirement.GroupKey == normalizedGroupKey
            && requirement.Kind == kind);

        if (alreadyExists)
        {
            return;
        }

        equipmentRequirements.Add(
            MovementEquipmentRequirement.New(
                Id,
                equipmentId,
                normalizedGroupKey,
                kind));

        MarkUpdated(changedBy);
    }

    public void RemoveEquipmentRequirement(
        EquipmentId equipmentId,
        string groupKey,
        EquipmentRequirementKind kind,
        User? changedBy = null)
    {
        var normalizedGroupKey = FormatingUtilities.NormalizeEquipmentGroupKey(groupKey);

        var requirement = equipmentRequirements
            .FirstOrDefault(requirement =>
                requirement.EquipmentId == equipmentId
                && requirement.GroupKey == normalizedGroupKey
                && requirement.Kind == kind);

        if (requirement is null)
        {
            return;
        }

        equipmentRequirements.Remove(requirement);

        MarkUpdated(changedBy);
    }

    public void MakeVariantOf(MovementId parentMovementId, User? changedBy = null)
    {
        if (parentMovementId == Id)
        {
            throw new ArgumentException("A movement cannot be a variant of itself.");
        }

        VariantOfId = parentMovementId;
        MarkUpdated(changedBy);
    }

    public void RemoveVariantParent(User? changedBy = null)
    {
        if (VariantOfId is null)
        {
            return;
        }

        VariantOfId = null;
        VariantOf = null;

        MarkUpdated(changedBy);
    }

    public void AddVariant(Movement variant, User? changedBy = null)
    {
        if (variant.Id == Id)
        {
            throw new ArgumentException("A movement cannot be a variant of itself.");
        }

        if (VariantIDs.Contains(variant.Id))
        {
            return;
        }

        variant.MakeVariantOf(Id, changedBy);

        variants.Add(variant);

        MarkUpdated(changedBy);
    }

    public void RemoveVariant(MovementId variantId, User? changedBy = null)
    {
        if (!VariantIDs.Contains(variantId))
        {
            return;
        }

        var variant = variants
            .FirstOrDefault(variant => variant.Id == variantId);

        if (variant is null)
        {
            return;
        }

        variant.RemoveVariantParent(changedBy);

        variants.Remove(variant);

        MarkUpdated(changedBy);
    }
    
    public void AddPattern(
        MovementPattern pattern,
        User? changedBy = null)
    {
        if (pattern == MovementPattern.Complex)
        {
            throw new ArgumentException(
                "Complex should be used as a primary pattern summary, not as a specific pattern tag.",
                nameof(pattern));
        }

        if (PatternValues.Contains(pattern))
        {
            return;
        }

        patterns.Add(
            MovementPatternTag.New(Id, pattern)
        );

        AutoResolvePrimaryPatternWithoutAudit();

        MarkUpdated(changedBy);
    }

    public void RemovePattern(
        MovementPattern pattern,
        User? changedBy = null)
    {
        if (!PatternValues.Contains(pattern))
        {
            return;
        }

        var patternTag = patterns
            .FirstOrDefault(patternTag => patternTag.Pattern == pattern);

        if (patternTag is null)
        {
            return;
        }

        patterns.Remove(patternTag);

        AutoResolvePrimaryPatternWithoutAudit();

        MarkUpdated(changedBy);
    }

    public void SetPrimaryPattern(
        MovementPattern pattern,
        User? changedBy = null)
    {
        if (pattern != MovementPattern.Complex
            && !PatternValues.Contains(pattern))
        {
            patterns.Add(
                MovementPatternTag.New(Id, pattern)
            );
        }

        PrimaryPattern = pattern;
        MarkUpdated(changedBy);
    }

    public void ClearPrimaryPattern(User? changedBy = null)
    {
        if (PrimaryPattern is null)
        {
            return;
        }

        PrimaryPattern = null;
        MarkUpdated(changedBy);
    }

    public void AutoResolvePrimaryPattern(User? changedBy = null)
    {
        var resolvedPrimaryPattern = ResolvePrimaryPatternOrNull();

        if (PrimaryPattern == resolvedPrimaryPattern)
        {
            return;
        }

        PrimaryPattern = resolvedPrimaryPattern;
        MarkUpdated(changedBy);
    }

    public bool Update(MovementUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var hasChanged = false;

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            hasChanged = TrySetName(update.Name) || hasChanged;
        }

        if (update.MovementCategoryId.HasValue
            && MovementCategoryId != update.MovementCategoryId.Value)
        {
            MovementCategoryId = update.MovementCategoryId.Value;
            hasChanged = true;
        }
        
        if (update.Laterality.HasValue
            && Laterality != update.Laterality.Value)
        {
            Laterality = update.Laterality.Value;
            hasChanged = true;
        }

        if (update.Description is not null)
        {
            Description = Description.ChangeContent(
                update.Description.Content,
                update.Description.Language);

            hasChanged = true;
        }

        if (update.DefaultWorkTargetType.HasValue)
        {
            if (DefaultWorkTargetType != update.DefaultWorkTargetType.Value)
            {
                SetDefaultWorkTargetType(
                    update.DefaultWorkTargetType.Value,
                    update.UpdatedBy);

                hasChanged = true;
            }
        }

        if (update.AllowedWorkTargetTypes is not null)
        {
            hasChanged |= ReplaceAllowedWorkTargetTypes(update.AllowedWorkTargetTypes);
        }
        
        if (update.EquipmentRequirements is not null)
        {
            hasChanged |= ReplaceEquipments(update.EquipmentRequirements);
        }

        if (update.Muscles is not null)
        {
            hasChanged |= ReplaceMuscles(update.Muscles);
        }

        if (update.Patterns is not null)
        {
            hasChanged |= ReplacePatterns(update.Patterns);
        }

        if (update.PrimaryPattern.HasValue)
        {
            if (PrimaryPattern != update.PrimaryPattern.Value)
            {
                SetPrimaryPattern(update.PrimaryPattern.Value);
                hasChanged = true;
            }
        }
        else if (update.Patterns is not null)
        {
            var previousPrimaryPattern = PrimaryPattern;

            AutoResolvePrimaryPattern();

            hasChanged |= previousPrimaryPattern != PrimaryPattern;
        }

        if (update.WasVariantOfProvided)
        {
            hasChanged |= ReplaceVariantOf(update.VariantOfId);
        }

        if (hasChanged)
        {
            MarkUpdated(update.UpdatedBy);
        }

        return hasChanged;
    }
    
    private bool ReplaceAllowedWorkTargetTypes(
        IReadOnlyCollection<WorkTargetType> updatedTargetTypes)
    {
        var normalizedTargetTypes = updatedTargetTypes.ToHashSet();

        normalizedTargetTypes.Add(DefaultWorkTargetType);

        if (AllowedWorkTargetTypes.SetEquals(normalizedTargetTypes))
        {
            return false;
        }

        var targetTypesToRemove = allowedWorkTargets
            .Where(allowedWorkTarget => !normalizedTargetTypes.Contains(allowedWorkTarget.TargetType))
            .ToList();

        foreach (var allowedWorkTarget in targetTypesToRemove)
        {
            allowedWorkTargets.Remove(allowedWorkTarget);
        }

        var existingTargetTypes = allowedWorkTargets
            .Select(allowedWorkTarget => allowedWorkTarget.TargetType)
            .ToHashSet();

        var targetTypesToAdd = normalizedTargetTypes
            .Where(targetType => !existingTargetTypes.Contains(targetType))
            .ToList();

        foreach (var targetType in targetTypesToAdd)
        {
            allowedWorkTargets.Add(
                MovementAllowedWorkTarget.New(
                    Id,
                    targetType));
        }

        return true;
    }

    private MovementPattern? ResolvePrimaryPatternOrNull()
    {
        return patterns.Count switch
        {
            0 => null,
            1 => patterns[0].Pattern,
            _ => MovementPattern.Complex,
        };
    }
    
    private void AutoResolvePrimaryPatternWithoutAudit()
    {
        PrimaryPattern = ResolvePrimaryPatternOrNull();
    }
    
    private void SetPrimaryPatternWithoutAudit(MovementPattern pattern)
    {
        if (pattern != MovementPattern.Complex
            && !PatternValues.Contains(pattern))
        {
            patterns.Add(
                MovementPatternTag.New(Id, pattern)
            );
        }

        PrimaryPattern = pattern;
    }

    private bool ReplacePatterns(IReadOnlyCollection<MovementPatternTag> updatedPatterns)
    {
        var updatedPatternValues = updatedPatterns
            .Select(patternTag => patternTag.Pattern)
            .ToHashSet();

        if (updatedPatternValues.Contains(MovementPattern.Complex))
        {
            throw new ArgumentException(
                "Complex should be used as a primary pattern summary, not as a specific pattern tag.",
                nameof(updatedPatterns));
        }

        if (PatternValues.SetEquals(updatedPatternValues))
        {
            return false;
        }

        patterns.Clear();

        foreach (var pattern in updatedPatternValues)
        {
            patterns.Add(
                MovementPatternTag.New(Id, pattern)
            );
        }

        if (PrimaryPattern.HasValue
            && PrimaryPattern.Value != MovementPattern.Complex
            && !updatedPatternValues.Contains(PrimaryPattern.Value))
        {
            PrimaryPattern = ResolvePrimaryPatternOrNull();
        }

        return true;
    }

    private bool ReplaceVariantOf(MovementId? variantOfId)
    {
        if (variantOfId == Id)
        {
            throw new ArgumentException("A movement cannot be a variant of itself.");
        }

        if (VariantOfId == variantOfId)
        {
            return false;
        }

        VariantOfId = variantOfId;
        VariantOf = null;

        return true;
    }
    
    private bool ReplaceMuscles(IReadOnlyCollection<MovementMuscle> updatedMuscles)
    {
        var hasSameMuscles = Muscles.Count == updatedMuscles.Count
                             && Muscles.All(movementMuscle => updatedMuscles.Any(updatedMuscle =>
                                 updatedMuscle.MuscleId == movementMuscle.MuscleId
                                 && updatedMuscle.Role == movementMuscle.Role));

        if (hasSameMuscles)
        {
            return false;
        }

        muscles.Clear();

        foreach (var updatedMuscle in updatedMuscles)
        {
            muscles.Add(
                MovementMuscle.New(
                    Id,
                    updatedMuscle.MuscleId,
                    updatedMuscle.Role)
            );
        }

        return true;
    }

    private bool ReplaceEquipments(
        IReadOnlyCollection<MovementEquipmentRequirement> updatedEquipments)
    {
        var currentEquipmentRequirements = equipmentRequirements
            .Select(CreateEquipmentRequirementKey)
            .ToHashSet();

        var updatedEquipmentRequirements = updatedEquipments
            .Select(CreateEquipmentRequirementKey)
            .ToHashSet();

        if (currentEquipmentRequirements.SetEquals(updatedEquipmentRequirements))
        {
            return false;
        }

        equipmentRequirements.Clear();

        foreach (var updatedEquipment in updatedEquipments)
        {
            equipmentRequirements.Add(
                MovementEquipmentRequirement.New(
                    Id,
                    updatedEquipment.EquipmentId,
                    updatedEquipment.GroupKey,
                    updatedEquipment.Kind));
        }

        return true;
    }

    private static EquipmentRequirementKey CreateEquipmentRequirementKey(
        MovementEquipmentRequirement requirement)
    {
        return new EquipmentRequirementKey(
            requirement.EquipmentId,
            requirement.GroupKey,
            requirement.Kind);
    }

    private readonly record struct EquipmentRequirementKey(
        EquipmentId EquipmentId,
        string GroupKey,
        EquipmentRequirementKind Kind);

    private bool ReplaceVariants(
        IReadOnlyCollection<Movement> updatedVariants,
        User? changedBy = null)
    {
        if (updatedVariants.Any(variant => variant.Id == Id))
        {
            throw new ArgumentException("A movement cannot be a variant of itself.");
        }

        var updatedVariantIDs = updatedVariants
            .Select(variant => variant.Id)
            .ToHashSet();

        if (VariantIDs.SetEquals(updatedVariantIDs))
        {
            return false;
        }

        foreach (var variant in variants)
        {
            variant.RemoveVariantParent(changedBy);
        }

        variants.Clear();

        foreach (var updatedVariant in updatedVariants)
        {
            updatedVariant.MakeVariantOf(Id, changedBy);
            variants.Add(updatedVariant);
        }

        return true;
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