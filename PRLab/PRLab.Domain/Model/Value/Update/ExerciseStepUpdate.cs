using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription;

namespace PRLab.Domain.Model.Value.Update;

public sealed class ExerciseStepUpdate
{
    public MovementId MovementId { get; init; }

    public int Sequence { get; init; }

    public WorkTarget Target { get; init; } = null!;

    public LoadTarget? LoadTarget { get; init; }

    public RestTarget? RestBetweenReps { get; init; }

    public RestTarget? TransitionAfterStep { get; init; }

    public RepExecutionDetails? ExecutionDetails { get; init; }

    public static ExerciseStepUpdate FromExerciseStep(ExerciseSteps exerciseSteps)
    {
        ArgumentNullException.ThrowIfNull(exerciseSteps);

        return new ExerciseStepUpdate
        {
            MovementId = exerciseSteps.MovementId,
            Sequence = exerciseSteps.Sequence,
            Target = exerciseSteps.Target,
            LoadTarget = exerciseSteps.LoadTarget,
            RestBetweenReps = exerciseSteps.RestBetweenReps,
            TransitionAfterStep = exerciseSteps.TransitionAfterStep,
            ExecutionDetails = exerciseSteps.ExecutionDetails
        };
    }
}