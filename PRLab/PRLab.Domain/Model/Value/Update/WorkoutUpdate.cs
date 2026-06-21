using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Update;

public sealed class WorkoutUpdate
{
    public string? Name { get; init; }

    public DescriptionUpdate? Description { get; init; }

    public EstimatedDuration? EstimatedDuration { get; init; }

    public bool WasEstimatedDurationProvided { get; init; }

    public IReadOnlyCollection<WorkoutBlockAssignmentUpdate>? Blocks { get; init; }

    public User? UpdatedBy { get; init; }

    public static WorkoutUpdate FromWorkout(
        Workout workout,
        User? user)
    {
        ArgumentNullException.ThrowIfNull(workout);

        return new WorkoutUpdate
        {
            Name = workout.Name,
            Description = DescriptionUpdate.FromDescription(
                workout.Description,
                (LocalizationHelper.Language?)null,
                user),
            EstimatedDuration = workout.EstimatedDuration,
            WasEstimatedDurationProvided = true,
            Blocks = workout.Blocks
                .Select(WorkoutBlockAssignmentUpdate.FromAssignment)
                .ToList(),
            UpdatedBy = user
        };
    }

    public static WorkoutUpdate FromWorkout(
        Workout workout,
        LocalizationHelper.Language? language,
        User? user)
    {
        ArgumentNullException.ThrowIfNull(workout);

        return new WorkoutUpdate
        {
            Name = workout.Name,
            Description = DescriptionUpdate.FromDescription(
                workout.Description,
                language,
                user),
            EstimatedDuration = workout.EstimatedDuration,
            WasEstimatedDurationProvided = true,
            Blocks = workout.Blocks
                .Select(WorkoutBlockAssignmentUpdate.FromAssignment)
                .ToList(),
            UpdatedBy = user
        };
    }
}

public sealed class WorkoutBlockAssignmentUpdate
{
    public int Sequence { get; init; }

    public WorkoutBlock WorkoutBlock { get; init; } = null!;

    public static WorkoutBlockAssignmentUpdate FromAssignment(
        WorkoutBlockAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        return new WorkoutBlockAssignmentUpdate
        {
            Sequence = assignment.Sequence,
            WorkoutBlock = assignment.WorkoutBlock
        };
    }
}