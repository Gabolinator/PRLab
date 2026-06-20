using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Domain.Model.Value;

public sealed record ExerciseSteps
{
    public ExerciseStepsId Id { get; init; }

    public ExerciseId ExerciseId { get; private set; }

    public MovementId MovementId { get; private set; }

    public Movement Movement { get; private set; } = null!;

    public int Sequence { get; private set; }

    public WorkTarget Target { get; private set; } = null!;

    public LoadTarget LoadTarget { get; private set; } = null!;

    public RestTarget RestBetweenReps { get; private set; } = null!;

    public RestTarget TransitionAfterStep { get; private set; } = null!;

    public RepExecutionDetails ExecutionDetails { get; private set; } = null!;

    private ExerciseSteps()
    {
        // EF Core
    }

    private ExerciseSteps(
        ExerciseStepsId id,
        ExerciseId exerciseId,
        MovementId movementId,
        int sequence,
        WorkTarget target,
        LoadTarget loadTarget,
        RestTarget restBetweenReps,
        RestTarget transitionAfterStep,
        RepExecutionDetails executionDetails)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(loadTarget);
        ArgumentNullException.ThrowIfNull(restBetweenReps);
        ArgumentNullException.ThrowIfNull(transitionAfterStep);
        ArgumentNullException.ThrowIfNull(executionDetails);

        Id = id;
        ExerciseId = exerciseId;
        MovementId = movementId;
        Sequence = ValidateSequence(sequence);
        Target = target;
        LoadTarget = loadTarget;
        RestBetweenReps = restBetweenReps;
        TransitionAfterStep = transitionAfterStep;
        ExecutionDetails = executionDetails;
    }

    public static ExerciseSteps New(
        ExerciseId exerciseId,
        MovementId movementId,
        int sequence,
        WorkTarget target,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterStep = null,
        RepExecutionDetails? executionDetails = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        return new ExerciseSteps(
            ExerciseStepsId.New(),
            exerciseId,
            movementId,
            sequence,
            target,
            loadTarget ?? LoadTarget.None(),
            restBetweenReps ?? RestTarget.None(),
            transitionAfterStep ?? RestTarget.None(),
            executionDetails ?? RepExecutionDetails.Empty()
        );
    }

    public void ChangeTarget(WorkTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        Target = target;
    }

    public void ChangeLoadTarget(LoadTarget loadTarget)
    {
        ArgumentNullException.ThrowIfNull(loadTarget);

        LoadTarget = loadTarget;
    }

    public void RemoveLoadTarget()
    {
        LoadTarget = LoadTarget.None();
    }

    public void ChangeRestBetweenReps(RestTarget restBetweenReps)
    {
        ArgumentNullException.ThrowIfNull(restBetweenReps);

        RestBetweenReps = restBetweenReps;
    }

    public void RemoveRestBetweenReps()
    {
        RestBetweenReps = RestTarget.None();
    }

    public void ChangeTransitionAfterStep(RestTarget transitionAfterStep)
    {
        ArgumentNullException.ThrowIfNull(transitionAfterStep);

        TransitionAfterStep = transitionAfterStep;
    }

    public void RemoveTransitionAfterStep()
    {
        TransitionAfterStep = RestTarget.None();
    }

    public void ChangeExecutionDetails(RepExecutionDetails executionDetails)
    {
        ArgumentNullException.ThrowIfNull(executionDetails);

        ExecutionDetails = executionDetails;
    }

    public void RemoveExecutionDetails()
    {
        ExecutionDetails = RepExecutionDetails.Empty();
    }

    public void ChangeSequence(int sequence)
    {
        Sequence = ValidateSequence(sequence);
    }

    private static int ValidateSequence(int sequence)
    {
        if (sequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.");
        }

        return sequence;
    }
}