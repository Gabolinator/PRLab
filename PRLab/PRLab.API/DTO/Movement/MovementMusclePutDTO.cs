using PRLab.Domain;
using PRLab.Domain.Value.Enum.Anatomy;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Movement;

public sealed record MovementMusclePutDTO
{
    public required MuscleId MuscleId { get; init; }

    public required MuscleRole Role { get; init; }
}