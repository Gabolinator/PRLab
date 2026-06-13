using PRLab.Domain;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Value.Update;

public sealed record MovementEquipmentRequirementUpdate
{
    public required string GroupKey { get; init; }

    public required EquipmentRequirementKind Kind { get; init; }

    public required IReadOnlyList<EquipmentId> EquipmentIds { get; init; }
}