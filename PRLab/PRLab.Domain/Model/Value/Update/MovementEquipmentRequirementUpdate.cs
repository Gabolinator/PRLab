using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Value.Update;

public sealed record MovementEquipmentRequirementUpdate
{
    public required string GroupKey { get; init; }

    public required EquipmentRequirementKind Kind { get; init; }

    public required IReadOnlyList<EquipmentId> EquipmentIds { get; init; }
}