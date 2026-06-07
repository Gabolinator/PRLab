using PRLab.Domain;
using PRLab.Domain.Model.Join;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement;

public sealed record MovementMuscleSeedJsonDto
{
    public Guid? Id { get; init; }

    public string? Name { get; init; }

    public string? NameKey { get; init; }

    public DomainEnum.MuscleRole Role { get; init; } = DomainEnum.MuscleRole.Secondary;

    public static MovementMuscleSeedJsonDto FromMovementMuscle(MovementMuscle muscle) =>
        new()
        {
            Id = muscle.Muscle.Id.Value,
            Name = muscle.Muscle.Name,
            NameKey = muscle.Muscle.NameKey,
            Role = muscle.Role,
        };
}