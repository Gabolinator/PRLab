using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Movement.Relation;

public sealed record MovementEquipmentRequirementPutDTO
{
    public required string GroupKey { get; init; }

    public required DomainEnum.EquipmentRequirementKind Kind { get; init; }

    public required IReadOnlyList<EquipmentId> EquipmentIds { get; init; }
}