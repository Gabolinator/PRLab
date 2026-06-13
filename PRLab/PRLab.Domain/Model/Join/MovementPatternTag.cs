using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Identifier;

public sealed record MovementPatternTag
{
    public MovementId MovementId { get; init; }
    
    public MovementPattern Pattern { get; init; }

    private MovementPatternTag()
    {
        // EF Core
    }

    private MovementPatternTag(
        MovementId movementId,
        MovementPattern pattern)
    {
        MovementId = movementId;
        Pattern = pattern;
    }

    public static MovementPatternTag New(
        MovementId movementId,
        MovementPattern pattern)
    {
        return new MovementPatternTag(
            movementId,
            pattern
        );
    }
}