using PRLab.API.DTO.Description;
using PRLab.API.DTO.Movement.Relation;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Movement;

public sealed record MovementPutDTO
{
    public required string Name { get; init; }

    public required MovementCategoryId MovementCategoryId { get; init; }

    public DescriptionPutDTO? Description { get; init; }

    public required WorkTargetType DefaultWorkTargetType { get; init; }

    public IReadOnlyList<WorkTargetType> AllowedWorkTargetTypes { get; init; } = [];

    public IReadOnlyList<MovementEquipmentRequirementPutDTO> EquipmentRequirements { get; init; } = [];

    public IReadOnlyList<MovementMusclePutDTO> Muscles { get; init; } = [];

    public MovementPattern? PrimaryPattern { get; init; }

    public IReadOnlyList<MovementPattern> Patterns { get; init; } = [];

    public MovementId? VariantOfMovementId { get; init; }
}