using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Movement.Relation;

public sealed record MovementMusclePostDTO
{
    public required MuscleId MuscleId { get; init; }

    public required DomainEnum.MuscleRole Role { get; init; }
}