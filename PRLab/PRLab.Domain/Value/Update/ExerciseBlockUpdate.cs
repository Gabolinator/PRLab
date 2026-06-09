using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Value.Update;

public sealed class ExerciseBlockUpdate
{
    public MovementId MovementId { get; init; }

    public int Sequence { get; init; }

    public WorkTarget Target { get; init; } = null!;

    public LoadTarget? LoadTarget { get; init; }

    public RestTarget? RestBetweenReps { get; init; }

    public RestTarget? TransitionAfterBlock { get; init; }

    public RepExecutionDetails? ExecutionDetails { get; init; }

    public static ExerciseBlockUpdate FromExerciseBlock(ExerciseBlock exerciseBlock)
    {
        ArgumentNullException.ThrowIfNull(exerciseBlock);

        return new ExerciseBlockUpdate
        {
            MovementId = exerciseBlock.MovementId,
            Sequence = exerciseBlock.Sequence,
            Target = exerciseBlock.Target,
            LoadTarget = exerciseBlock.LoadTarget,
            RestBetweenReps = exerciseBlock.RestBetweenReps,
            TransitionAfterBlock = exerciseBlock.TransitionAfterBlock,
            ExecutionDetails = exerciseBlock.ExecutionDetails
        };
    }
}