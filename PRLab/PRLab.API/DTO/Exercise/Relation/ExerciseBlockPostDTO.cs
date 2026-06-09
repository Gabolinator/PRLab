using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record ExerciseBlockPostDTO
{
    public required MovementId MovementId { get; init; }

    public required int Sequence { get; init; }

    public required WorkTargetPostDTO Target { get; init; }

    public LoadTargetPostDTO? LoadTarget { get; init; }

    public RestTargetPostDTO? RestBetweenReps { get; init; }

    public RestTargetPostDTO? TransitionAfterBlock { get; init; }

    public RepExecutionDetailsPostDTO? ExecutionDetails { get; init; }
}