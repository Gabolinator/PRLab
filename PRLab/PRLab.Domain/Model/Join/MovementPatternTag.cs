using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

public sealed record MovementPatternTag
{
    public MovementId MovementId { get; init; }

    public DomainEnum.MovementPattern Pattern { get; init; }

    private MovementPatternTag()
    {
        // EF Core
    }

    private MovementPatternTag(
        MovementId movementId,
        DomainEnum.MovementPattern pattern)
    {
        MovementId = movementId;
        Pattern = pattern;
    }

    public static MovementPatternTag New(
        MovementId movementId,
        DomainEnum.MovementPattern pattern)
    {
        return new MovementPatternTag(
            movementId,
            pattern
        );
    }
}