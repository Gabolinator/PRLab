using PRLab.API.DTO.Description;
using PRLab.API.DTO.Movement.Relation;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Movement;

public sealed record MovementPutDTO
{
    public required string Name { get; init; }

    public required MovementCategoryId MovementCategoryId { get; init; }

    public DescriptionPutDTO? Description { get; init; }

    public IReadOnlyList<MovementEquipmentRequirementPutDTO> EquipmentRequirements { get; init; } = [];

    public IReadOnlyList<MovementMusclePutDTO> Muscles { get; init; } = [];

    public DomainEnum.MovementPattern? PrimaryPattern { get; init; }

    public IReadOnlyList<DomainEnum.MovementPattern> Patterns { get; init; } = [];

    public MovementId? VariantOfMovementId { get; init; }
}