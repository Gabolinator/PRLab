using PRLab.API.DTO.Equipment;
using PRLab.Domain;
using PRLab.Domain.Value.Enum.Movement;

namespace PRLab.API.DTO.Movement.Relation;

public sealed record MovementEquipmentRequirementGetDTO(
    string GroupKey,
    EquipmentRequirementKind Kind,
    IReadOnlyList<EquipmentSummaryDTO> Options);