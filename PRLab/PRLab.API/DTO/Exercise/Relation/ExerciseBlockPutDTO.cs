using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record ExerciseBlockPutDTO
{
    public required MovementId MovementId { get; init; }

    public required int Sequence { get; init; }

    public required WorkTargetPutDTO Target { get; init; }

    public LoadTargetPutDTO? LoadTarget { get; init; }

    public RestTargetPutDTO? RestBetweenReps { get; init; }

    public RestTargetPutDTO? TransitionAfterBlock { get; init; }

    public RepExecutionDetailsPutDTO? ExecutionDetails { get; init; }
}