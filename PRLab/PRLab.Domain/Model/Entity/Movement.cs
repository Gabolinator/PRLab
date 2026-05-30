using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Entity;

public sealed record Movement : IAudited, IDescribed
{
    public MovementId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public string NameKey { get; private set; } = string.Empty;

    public MovementCategoryId MovementCategoryId { get; private set; }

    public MovementCategory MovementCategory { get; private set; } = null!;

    public MovementId? VariantOfId { get; private set; }

    public Movement? VariantOf { get; private set; }

    private readonly List<Movement> variants = [];

    public IReadOnlyCollection<Movement> Variants => variants;
    
    public DomainEnum.MovementPattern? PrimaryPattern { get; private set; }

    private readonly List<MovementPatternTag> patterns = [];

    public IReadOnlyCollection<MovementPatternTag> Patterns => patterns;

    private HashSet<DomainEnum.MovementPattern> PatternValues => Patterns
        .Select(patternTag => patternTag.Pattern)
        .ToHashSet();

    private readonly List<MovementMuscle> muscles = [];

    public IReadOnlyCollection<MovementMuscle> Muscles => muscles;

    private readonly List<MovementEquipment> equipments = [];
    
    public IReadOnlyCollection<MovementEquipment> Equipments => equipments;

    public Description Description { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    private HashSet<MovementId> VariantIDs => Variants
        .Select(variant => variant.Id)
        .ToHashSet();

    private HashSet<MuscleId> MuscleIDs => Muscles
        .Select(movementMuscle => movementMuscle.MuscleId)
        .ToHashSet();

    private HashSet<MuscleId> PrimaryMuscleIDs => Muscles
        .Where(movementMuscle => movementMuscle.Role == DomainEnum.MuscleRole.Primary)
        .Select(movementMuscle => movementMuscle.MuscleId)
        .ToHashSet();

    private HashSet<MuscleId> SecondaryMuscleIDs => Muscles
        .Where(movementMuscle => movementMuscle.Role == DomainEnum.MuscleRole.Secondary)
        .Select(movementMuscle => movementMuscle.MuscleId)
        .ToHashSet();

    private HashSet<EquipmentId> EquipmentIDs => Equipments
        .Select(movementEquipment => movementEquipment.EquipmentId)
        .ToHashSet();

    private Movement()
    {
        // EF Core
    }

    private Movement(
        MovementId id,
        string name,
        MovementCategoryId movementCategoryId,
        Description description,
        AuditInfo audit)
    {
        Id = id;
        SetName(name);
        MovementCategoryId = movementCategoryId;
        Description = description;
        Audit = audit;
    }

    public static Movement New(
        string name,
        MovementCategoryId movementCategoryId,
        string? description,
        User? createdBy = null)
    {
        return new Movement(
            MovementId.New(),
            name,
            movementCategoryId,
            Description.New(description),
            AuditInfo.New(createdBy)
        );
    }
    
    private void SetName(string name)
    {
        Name = FormatingUtilities.NormalizeName(name);
        NameKey = FormatingUtilities.NormalizeNameKey(name);
    }

    public void Rename(string name, User? changedBy = null)
    {
        SetName(name);
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
            DomainEnum.MuscleRole.Primary,
            changedBy
        );
    }

    public void AddSecondaryMuscle(MuscleId muscleId, User? changedBy = null)
    {
        AddMuscle(
            muscleId,
            DomainEnum.MuscleRole.Secondary,
            changedBy
        );
    }

    public void AddMuscle(
        MuscleId muscleId,
        DomainEnum.MuscleRole role,
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
        DomainEnum.MuscleRole role,
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

    public void AddEquipment(EquipmentId equipmentId, User? changedBy = null)
    {
        if (EquipmentIDs.Contains(equipmentId))
        {
            return;
        }

        equipments.Add(
            MovementEquipment.New(
                Id,
                equipmentId
            )
        );

        MarkUpdated(changedBy);
    }

    public void RemoveEquipment(EquipmentId equipmentId, User? changedBy = null)
    {
        if (!EquipmentIDs.Contains(equipmentId))
        {
            return;
        }

        var movementEquipment = equipments
            .FirstOrDefault(movementEquipment => movementEquipment.EquipmentId == equipmentId);

        if (movementEquipment is null)
        {
            return;
        }

        equipments.Remove(movementEquipment);

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
        DomainEnum.MovementPattern pattern,
        User? changedBy = null)
    {
        if (pattern == DomainEnum.MovementPattern.Complex)
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

        MarkUpdated(changedBy);
    }

    public void RemovePattern(
        DomainEnum.MovementPattern pattern,
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

        if (PrimaryPattern == pattern)
        {
            PrimaryPattern = ResolvePrimaryPatternOrNull();
        }

        MarkUpdated(changedBy);
    }

    public void SetPrimaryPattern(
        DomainEnum.MovementPattern pattern,
        User? changedBy = null)
    {
        if (pattern != DomainEnum.MovementPattern.Complex
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
        PrimaryPattern = ResolvePrimaryPatternOrNull();
        MarkUpdated(changedBy);
    }

    private DomainEnum.MovementPattern? ResolvePrimaryPatternOrNull()
    {
        return patterns.Count switch
        {
            0 => null,
            1 => patterns[0].Pattern,
            _ => DomainEnum.MovementPattern.Complex,
        };
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