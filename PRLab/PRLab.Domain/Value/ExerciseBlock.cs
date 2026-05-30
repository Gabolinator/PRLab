using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Value;

public sealed record ExerciseBlock
{
    public ExerciseBlockId Id { get; init; }

    public ExerciseId ExerciseId { get; private set; }

    public MovementId MovementId { get; private set; }

    public Movement Movement { get; private set; } = null!;

    public int Sequence { get; private set; }

    public WorkTarget Target { get; private set; } = null!;

    public LoadTarget LoadTarget { get; private set; } = null!;

    public RestTarget RestBetweenReps { get; private set; } = null!;

    public RestTarget TransitionAfterBlock { get; private set; } = null!;

    public RepExecutionDetails ExecutionDetails { get; private set; } = null!;

    private ExerciseBlock()
    {
        // EF Core
    }

    private ExerciseBlock(
        ExerciseBlockId id,
        ExerciseId exerciseId,
        MovementId movementId,
        int sequence,
        WorkTarget target,
        LoadTarget loadTarget,
        RestTarget restBetweenReps,
        RestTarget transitionAfterBlock,
        RepExecutionDetails executionDetails)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(loadTarget);
        ArgumentNullException.ThrowIfNull(restBetweenReps);
        ArgumentNullException.ThrowIfNull(transitionAfterBlock);
        ArgumentNullException.ThrowIfNull(executionDetails);

        Id = id;
        ExerciseId = exerciseId;
        MovementId = movementId;
        Sequence = ValidateSequence(sequence);
        Target = target;
        LoadTarget = loadTarget;
        RestBetweenReps = restBetweenReps;
        TransitionAfterBlock = transitionAfterBlock;
        ExecutionDetails = executionDetails;
    }

    public static ExerciseBlock New(
        ExerciseId exerciseId,
        MovementId movementId,
        int sequence,
        WorkTarget target,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterBlock = null,
        RepExecutionDetails? executionDetails = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        return new ExerciseBlock(
            ExerciseBlockId.New(),
            exerciseId,
            movementId,
            sequence,
            target,
            loadTarget ?? LoadTarget.None(),
            restBetweenReps ?? RestTarget.None(),
            transitionAfterBlock ?? RestTarget.None(),
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

    public void ChangeTransitionAfterBlock(RestTarget transitionAfterBlock)
    {
        ArgumentNullException.ThrowIfNull(transitionAfterBlock);

        TransitionAfterBlock = transitionAfterBlock;
    }

    public void RemoveTransitionAfterBlock()
    {
        TransitionAfterBlock = RestTarget.None();
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