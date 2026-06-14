using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Join;

public sealed record MovementAllowedWorkTarget
{
    public MovementId MovementId { get; init; }

    public WorkTargetType TargetType { get; private set; }

    private MovementAllowedWorkTarget()
    {
        // EF Core
    }

    private MovementAllowedWorkTarget(
        MovementId movementId,
        WorkTargetType targetType)
    {
        if (movementId.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(movementId));
        }

        MovementId = movementId;
        TargetType = targetType;
    }

    public static MovementAllowedWorkTarget New(
        MovementId movementId,
        WorkTargetType targetType)
    {
        return new MovementAllowedWorkTarget(
            movementId,
            targetType);
    }
}