using PRLab.Domain;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Value.Enum.Anatomy;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement;

public sealed record MovementMuscleSeedJsonDto
{
    public Guid? Id { get; init; }

    public string? Name { get; init; }

    public string? NameKey { get; init; }

    public MuscleRole Role { get; init; } = MuscleRole.Secondary;

    public static MovementMuscleSeedJsonDto FromMovementMuscle(MovementMuscle muscle) =>
        new()
        {
            Id = muscle.Muscle.Id.Value,
            Name = muscle.Muscle.Name,
            NameKey = muscle.Muscle.NameKey,
            Role = muscle.Role,
        };
}