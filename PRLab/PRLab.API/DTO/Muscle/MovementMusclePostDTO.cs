using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Muscle;

public sealed record MovementMusclePostDTO
{
    public required MuscleId MuscleId { get; init; }

    public required DomainEnum.MuscleRole Role { get; init; }
}