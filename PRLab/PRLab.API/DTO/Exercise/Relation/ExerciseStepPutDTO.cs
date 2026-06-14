using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record ExerciseStepPutDTO
{
    public required MovementId MovementId { get; init; }

    public required int Sequence { get; init; }

    public required WorkTargetPutDTO Target { get; init; }

    public LoadTargetPutDTO? LoadTarget { get; init; }

    public RestTargetPutDTO? RestBetweenReps { get; init; }

    public RestTargetPutDTO? TransitionAfterStep { get; init; }

    public RepExecutionDetailsPutDTO? ExecutionDetails { get; init; }
}