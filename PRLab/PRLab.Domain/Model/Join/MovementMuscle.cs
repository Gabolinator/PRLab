using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Join;

public sealed class MovementMuscle
{
    public MovementId MovementId { get; private init; }

    public Movement Movement { get; private init; } = null!;

    public MuscleId MuscleId { get; private init; }

    public Muscle Muscle { get; private init; } = null!;

    public DomainEnum.MuscleRole Role { get; private init; }

    private MovementMuscle()
    {
        // EF Core
    }

    private MovementMuscle(
        MovementId movementId,
        MuscleId muscleId,
        DomainEnum.MuscleRole role)
    {
        MovementId = movementId;
        MuscleId = muscleId;
        Role = role;
    }

    public static MovementMuscle New(
        MovementId movementId,
        MuscleId muscleId,
        DomainEnum.MuscleRole role)
    {
        return new MovementMuscle(
            movementId,
            muscleId,
            role
        );
    }
}