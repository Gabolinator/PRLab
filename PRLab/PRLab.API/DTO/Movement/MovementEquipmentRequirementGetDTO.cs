using PRLab.API.DTO.Equipment;
using PRLab.Domain;

namespace PRLab.API.DTO.Movement;

public sealed record MovementEquipmentRequirementGetDTO(
    string GroupKey,
    DomainEnum.EquipmentRequirementKind Kind,
    IReadOnlyList<EquipmentSummaryDTO> Options);