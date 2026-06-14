using PRLab.API.DTO.Description;
using PRLab.API.DTO.Movement.Relation;
using PRLab.API.DTO.MovementCategory;
using PRLab.API.DTO.Muscle;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Movement;

public sealed record MovementGetDTO(
    MovementId Id,
    string Name,
    DescriptionGetDTO? Description,
    MovementCategorySummaryDTO? Category,
    WorkTargetType DefaultWorkTargetType,
    IReadOnlyList<WorkTargetType> AllowedWorkTargetTypes,
    IReadOnlyList<MovementEquipmentRequirementGetDTO> EquipmentRequirements,
    IReadOnlyList<MuscleSummaryDTO> PrimaryMuscles,
    IReadOnlyList<MuscleSummaryDTO> SecondaryMuscles,
    MovementPattern? PrimaryPattern,
    IReadOnlyList<MovementPattern> Patterns,
    MovementSummaryDTO? VariantOfMovement);