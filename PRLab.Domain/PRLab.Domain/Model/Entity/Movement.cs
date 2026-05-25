using PRLab.Model.Interface;
using PRLab.Model.Join;
using PRLab.Utilities;
using PRLab.Value.Identifier;

namespace PRLab.Model.Entity;

public sealed record Movement : IAudited
{
    public MovementId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public MovementCategoryId MovementCategoryId { get; private set; }

    public MovementCategory MovementCategory { get; private set; } = null!;

    public MovementId? VariantOfId { get; private set; }

    public Movement? VariantOf { get; private set; }

    private readonly List<Movement> variants = [];

    public IReadOnlyCollection<Movement> Variants => variants;

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
        Name = FormatingUtilities.NormalizeName(name);
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

    public void Rename(string name, User? changedBy = null)
    {
        Name = FormatingUtilities.NormalizeName(name);
        Audit = Audit.MarkUpdated(changedBy);
    }

    public void ChangeCategory(MovementCategoryId movementCategoryId, User? changedBy = null)
    {
        MovementCategoryId = movementCategoryId;
        Audit = Audit.MarkUpdated(changedBy);
    }

    public void ChangeDescription(string? content, User? changedBy = null)
    {
        Description = Description.ChangeContent(content);
        Audit = Audit.MarkUpdated(changedBy);
    }

    public void RemoveDescription(User? changedBy = null)
    {
        Description = Description.RemoveContent();
        Audit = Audit.MarkUpdated(changedBy);
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

        Audit = Audit.MarkUpdated(changedBy);
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

        Audit = Audit.MarkUpdated(changedBy);
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

        Audit = Audit.MarkUpdated(changedBy);
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

        Audit = Audit.MarkUpdated(changedBy);
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

        Audit = Audit.MarkUpdated(changedBy);
    }

    public void MakeVariantOf(MovementId parentMovementId, User? changedBy = null)
    {
        if (parentMovementId == Id)
        {
            throw new ArgumentException("A movement cannot be a variant of itself.");
        }

        VariantOfId = parentMovementId;
        Audit = Audit.MarkUpdated(changedBy);
    }

    public void RemoveVariantParent(User? changedBy = null)
    {
        if (VariantOfId is null)
        {
            return;
        }

        VariantOfId = null;
        VariantOf = null;

        Audit = Audit.MarkUpdated(changedBy);
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

        Audit = Audit.MarkUpdated(changedBy);
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

        Audit = Audit.MarkUpdated(changedBy);
    }
}