using PRLab.API.DTO.Equipment;
using PRLab.Domain;

namespace PRLab.API.DTO.Movement.Relation;

public sealed record MovementEquipmentRequirementGetDTO(
    string GroupKey,
    DomainEnum.EquipmentRequirementKind Kind,
    IReadOnlyList<EquipmentSummaryDTO> Options);