using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.WorkoutValue;

namespace PRLab.Domain.Model.Join;

public sealed record WorkoutBlockAssignment
{
    public WorkoutBlockAssignmentId Id { get; init; }

    public WorkoutId WorkoutId { get; private set; }

    public WorkoutBlockId WorkoutBlockId { get; private set; }

    public WorkoutBlock WorkoutBlock { get; private set; } = null!;

    public int Sequence { get; private set; }

    private WorkoutBlockAssignment()
    {
        // EF Core
    }

    private WorkoutBlockAssignment(
        WorkoutBlockAssignmentId id,
        WorkoutId workoutId,
        WorkoutBlock workoutBlock,
        int sequence)
    {
        ArgumentNullException.ThrowIfNull(workoutBlock);

        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout block assignment id cannot be empty.", nameof(id));
        }

        if (workoutId.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout id cannot be empty.", nameof(workoutId));
        }

        if (sequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.", nameof(sequence));
        }

        Id = id;
        WorkoutId = workoutId;
        WorkoutBlockId = workoutBlock.Id;
        WorkoutBlock = workoutBlock;
        Sequence = sequence;
    }

    public static WorkoutBlockAssignment New(
        WorkoutId workoutId,
        WorkoutBlock workoutBlock,
        int sequence)
    {
        return new WorkoutBlockAssignment(
            WorkoutBlockAssignmentId.New(),
            workoutId,
            workoutBlock,
            sequence);
    }
    
    private WorkoutBlockAssignment(
        WorkoutBlockAssignmentId id,
        WorkoutId workoutId,
        WorkoutBlockId workoutBlockId,
        int sequence)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout block assignment id cannot be empty.", nameof(id));
        }

        if (workoutId.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout id cannot be empty.", nameof(workoutId));
        }

        if (workoutBlockId.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout block id cannot be empty.", nameof(workoutBlockId));
        }

        if (sequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.", nameof(sequence));
        }

        Id = id;
        WorkoutId = workoutId;
        WorkoutBlockId = workoutBlockId;
        Sequence = sequence;
    }

    public static WorkoutBlockAssignment New(
        WorkoutId workoutId,
        WorkoutBlockId workoutBlockId,
        int sequence)
    {
        return new WorkoutBlockAssignment(
            WorkoutBlockAssignmentId.New(),
            workoutId,
            workoutBlockId,
            sequence);
    }

    public void ChangeSequence(int sequence)
    {
        if (sequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.", nameof(sequence));
        }

        Sequence = sequence;
    }
}