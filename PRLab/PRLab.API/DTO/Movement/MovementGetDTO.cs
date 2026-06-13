using PRLab.API.DTO.Description;
using PRLab.API.DTO.Movement.Relation;
using PRLab.API.DTO.MovementCategory;
using PRLab.API.DTO.Muscle;
using PRLab.Domain;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Movement;

public sealed record MovementGetDTO(
    MovementId Id,
    string Name,
    DescriptionGetDTO? Description,
    MovementCategorySummaryDTO? Category,
    IReadOnlyList<MovementEquipmentRequirementGetDTO> EquipmentRequirements,
    IReadOnlyList<MuscleSummaryDTO> PrimaryMuscles,
    IReadOnlyList<MuscleSummaryDTO> SecondaryMuscles,
    MovementPattern? PrimaryPattern,
    IReadOnlyList<MovementPattern> Patterns,
    MovementSummaryDTO? VariantOfMovement);