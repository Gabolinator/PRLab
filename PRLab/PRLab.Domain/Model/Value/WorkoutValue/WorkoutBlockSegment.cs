using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription;

namespace PRLab.Domain.Model.Value.WorkoutValue;

public sealed record WorkoutBlockSegment
{
    public WorkoutBlockSegmentId Id { get; init; }

    public WorkoutBlockId WorkoutBlockId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int Sequence { get; private set; }

    public WorkMode WorkMode { get; private set; }

    public WorkIntent WorkIntent { get; private set; }

    public WorkoutScoreType ScoreType { get; private set; }

    public TimeConstraint? TimeConstraint { get; private set; }

    public RestTarget? RestAfterSegment { get; private set; }

    private readonly List<WorkoutBlockSegmentStep> steps = [];

    public IReadOnlyCollection<WorkoutBlockSegmentStep> Steps => steps
        .OrderBy(step => step.Sequence)
        .ToList();

    private WorkoutBlockSegment()
    {
        // EF Core
    }

    private WorkoutBlockSegment(
        WorkoutBlockSegmentId id,
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkMode workMode,
        WorkIntent workIntent,
        WorkoutScoreType scoreType,
        TimeConstraint? timeConstraint,
        RestTarget? restAfterSegment)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout block segment id cannot be empty.", nameof(id));
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
        WorkoutBlockId = workoutBlockId;
        Name = name.Trim();
        Sequence = sequence;
        WorkMode = workMode;
        WorkIntent = workIntent;
        ScoreType = scoreType;
        TimeConstraint = timeConstraint;
        RestAfterSegment = restAfterSegment;
    }

    public static WorkoutBlockSegment ForTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkIntent intent = WorkIntent.ForTime,
        RestTarget? restAfterSegment = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.ForTime,
            intent,
            WorkoutScoreType.TimeToComplete,
            null,
            restAfterSegment);
    }

    public static WorkoutBlockSegment ForTimeWithCap(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan cap,
        WorkIntent intent = WorkIntent.ForTime,
        RestTarget? restAfterSegment = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.ForTimeWithCap,
            intent,
            WorkoutScoreType.TimeToComplete,
            TimeConstraint.Cap(cap),
            restAfterSegment);
    }

    public static WorkoutBlockSegment Amrap(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkIntent intent = WorkIntent.MaxEffort,
        WorkoutScoreType scoreType = WorkoutScoreType.CompletedRoundsAndExtraReps,
        RestTarget? restAfterSegment = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.MaxWorkInTime,
            intent,
            scoreType,
            TimeConstraint.Window(duration),
            restAfterSegment);
    }

    public static WorkoutBlockSegment MaxCaloriesInTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkIntent intent = WorkIntent.MaxEffort,
        RestTarget? restAfterSegment = null)
    {
        return MaxWorkInTime(
            workoutBlockId,
            name,
            sequence,
            duration,
            WorkoutScoreType.Calories,
            intent,
            restAfterSegment);
    }

    public static WorkoutBlockSegment MaxDistanceInTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkIntent intent = WorkIntent.MaxEffort,
        RestTarget? restAfterSegment = null)
    {
        return MaxWorkInTime(
            workoutBlockId,
            name,
            sequence,
            duration,
            WorkoutScoreType.Distance,
            intent,
            restAfterSegment);
    }

    public static WorkoutBlockSegment MaxRepsInTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkIntent intent = WorkIntent.MaxEffort,
        RestTarget? restAfterSegment = null)
    {
        return MaxWorkInTime(
            workoutBlockId,
            name,
            sequence,
            duration,
            WorkoutScoreType.TotalReps,
            intent,
            restAfterSegment);
    }

    public static WorkoutBlockSegment MaxWorkInTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkoutScoreType scoreType,
        WorkIntent intent = WorkIntent.MaxEffort,
        RestTarget? restAfterSegment = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.MaxWorkInTime,
            intent,
            scoreType,
            TimeConstraint.Window(duration),
            restAfterSegment);
    }

    public static WorkoutBlockSegment ForQuality(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkMode workMode = WorkMode.FixedWork,
        WorkoutScoreType scoreType = WorkoutScoreType.Quality,
        RestTarget? restAfterSegment = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            workMode,
            WorkIntent.ForQuality,
            scoreType,
            null,
            restAfterSegment);
    }

    public static WorkoutBlockSegment FixedWork(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkIntent intent,
        WorkoutScoreType scoreType = WorkoutScoreType.Completed,
        RestTarget? restAfterSegment = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.FixedWork,
            intent,
            scoreType,
            null,
            restAfterSegment);
    }

    public void AddStep(WorkoutBlockSegmentStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        steps.Add(step);
        ResequenceSteps();
    }

    public void RemoveStep(WorkoutBlockSegmentStepId stepId)
    {
        var step = steps.FirstOrDefault(existingStep => existingStep.Id == stepId);

        if (step is null)
        {
            return;
        }

        steps.Remove(step);
        ResequenceSteps();
    }

    public void ChangeSequence(int sequence)
    {
        if (sequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.", nameof(sequence));
        }

        Sequence = sequence;
    }

    public void ChangeRestAfterSegment(RestTarget restAfterSegment)
    {
        ArgumentNullException.ThrowIfNull(restAfterSegment);

        RestAfterSegment = restAfterSegment;
    }

    public void RemoveRestAfterSegment()
    {
        RestAfterSegment = null;
    }

    private void ResequenceSteps()
    {
        var orderedSteps = steps
            .OrderBy(step => step.Sequence)
            .ToList();

        for (var index = 0; index < orderedSteps.Count; index++)
        {
            orderedSteps[index] = orderedSteps[index] with
            {
                Sequence = index + 1
            };
        }

        steps.Clear();
        steps.AddRange(orderedSteps);
    }
}