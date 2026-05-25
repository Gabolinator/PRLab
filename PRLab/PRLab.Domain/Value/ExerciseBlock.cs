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
        RepExecutionDetails executionDetails)
    {
        Id = id;
        ExerciseId = exerciseId;
        MovementId = movementId;
        Sequence = ValidateSequence(sequence);
        Target = target;
        ExecutionDetails = executionDetails;
    }

    public static ExerciseBlock New(
        ExerciseId exerciseId,
        MovementId movementId,
        int sequence,
        WorkTarget target,
        RepExecutionDetails? executionDetails = null)
    {
        return new ExerciseBlock(
            ExerciseBlockId.New(),
            exerciseId,
            movementId,
            sequence,
            target,
            executionDetails ?? RepExecutionDetails.Empty()
        );
    }

    public void ChangeTarget(WorkTarget target)
    {
        Target = target;
    }

    public void ChangeExecutionDetails(RepExecutionDetails executionDetails)
    {
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
