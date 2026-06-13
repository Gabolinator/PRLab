using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Identifier;

public sealed record MovementEquipmentRequirement
{
    public MovementId MovementId { get; init; }

    public EquipmentId EquipmentId { get; init; }

    public string GroupKey { get; init; } = string.Empty;

    public EquipmentRequirementKind Kind { get; init; }

    public Equipment Equipment { get; init; } = null!;

    private MovementEquipmentRequirement()
    {
        // EF Core
    }

    private MovementEquipmentRequirement(
        MovementId movementId,
        EquipmentId equipmentId,
        string groupKey,
        EquipmentRequirementKind kind)
    {
        MovementId = movementId;
        EquipmentId = equipmentId;
        GroupKey = groupKey.Trim().ToLowerInvariant();
        Kind = kind;
    }

    public static MovementEquipmentRequirement New(
        MovementId movementId,
        EquipmentId equipmentId,
        string groupKey,
        EquipmentRequirementKind kind)
    {
        if (string.IsNullOrWhiteSpace(groupKey))
        {
            throw new ArgumentException("Equipment group key cannot be empty.", nameof(groupKey));
        }

        return new MovementEquipmentRequirement(
            movementId,
            equipmentId,
            groupKey,
            kind);
    }
}