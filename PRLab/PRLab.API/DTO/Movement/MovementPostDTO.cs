using PRLab.API.DTO.Description;
using PRLab.API.DTO.Movement.Relation;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Enum.Prescription;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Movement;

public sealed record MovementPostDTO
{
    public required string Name { get; init; }

    public required MovementCategoryId MovementCategoryId { get; init; }

    public DescriptionPostDTO? Descriptor { get; init; }

    public required WorkTargetType DefaultWorkTargetType { get; init; }

    public IReadOnlyList<WorkTargetType> AllowedWorkTargetTypes { get; init; } = [];

    public IReadOnlyList<MovementEquipmentRequirementPostDTO> EquipmentRequirements { get; init; } = [];

    public IReadOnlyList<MovementMusclePostDTO> Muscles { get; init; } = [];

    public MovementPattern? PrimaryPattern { get; init; }

    public IReadOnlyList<MovementPattern> Patterns { get; init; } = [];

    public MovementId? VariantOfMovementId { get; init; }
}