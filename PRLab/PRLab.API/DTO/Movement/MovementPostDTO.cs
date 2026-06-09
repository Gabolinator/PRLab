using PRLab.API.DTO.Description;
using PRLab.API.DTO.Movement.Relation;
using PRLab.API.DTO.Muscle;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Movement;

public sealed record MovementPostDTO
{
    public required string Name { get; init; }

    public required MovementCategoryId MovementCategoryId { get; init; }

    public DescriptionPostDTO? Descriptor { get; init; }

    public IReadOnlyList<MovementEquipmentRequirementPostDTO> EquipmentRequirements { get; init; } = [];

    public IReadOnlyList<MovementMusclePostDTO> Muscles { get; init; } = [];

    public DomainEnum.MovementPattern? PrimaryPattern { get; init; }

    public IReadOnlyList<DomainEnum.MovementPattern> Patterns { get; init; } = [];

    public MovementId? VariantOfMovementId { get; init; }
}