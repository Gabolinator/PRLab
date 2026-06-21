using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Prescription.Time;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Intensity;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Domain.Model.Value.WorkoutValue;

public sealed record WorkoutBlockSegment
{
    public WorkoutBlockSegmentId Id { get; init; }

    public WorkoutBlockId WorkoutBlockId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int Sequence { get; private set; }

    public WorkMode WorkMode { get; private set; }

    public WorkIntentPrescription Intent { get; private set; } = null!;

    public WorkoutScoreType ScoreType { get; private set; }

    public TimeConstraint? TimeConstraint { get; private set; }

    public IntervalPrescription? IntervalPrescription { get; private set; }

    public EstimatedDuration? EstimatedSegmentDuration { get; private set; }

    public RestTarget? RestAfterStep { get; private set; }

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
        WorkIntentPrescription intent,
        WorkoutScoreType scoreType,
        TimeConstraint? timeConstraint,
        IntervalPrescription? intervalPrescription,
        EstimatedDuration? estimatedSegmentDuration,
        RestTarget? restAfterStep,
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

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workout block segment name cannot be empty.", nameof(name));
        }

        if (sequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.", nameof(sequence));
        }

        ArgumentNullException.ThrowIfNull(intent);

        ValidateIntervalPrescription(
            workMode,
            intervalPrescription);

        Id = id;
        WorkoutBlockId = workoutBlockId;
        Name = name.Trim();
        Sequence = sequence;
        WorkMode = workMode;
        Intent = intent;
        ScoreType = scoreType;
        TimeConstraint = timeConstraint;
        IntervalPrescription = intervalPrescription;
        RestAfterSegment = restAfterSegment;
        RestAfterStep = restAfterStep;
        EstimatedSegmentDuration = estimatedSegmentDuration;
    }

    public static WorkoutBlockSegment New(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkMode workMode,
        WorkIntentPrescription intent,
        WorkoutScoreType scoreType,
        TimeConstraint? timeConstraint = null,
        IntervalPrescription? intervalPrescription = null,
        EstimatedDuration? estimatedSegmentDuration = null,
        RestTarget? restAfterStep = null,
        RestTarget? restAfterSegment = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            workMode,
            intent,
            scoreType,
            timeConstraint,
            intervalPrescription,
            estimatedSegmentDuration,
            restAfterStep,
            restAfterSegment);
    }
    
    public static WorkoutBlockSegment NewWithId(
        WorkoutBlockSegmentId id,
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkMode workMode,
        WorkIntentPrescription intent,
        WorkoutScoreType scoreType,
        TimeConstraint? timeConstraint = null,
        IntervalPrescription? intervalPrescription = null,
        EstimatedDuration? estimatedSegmentDuration = null,
        RestTarget? restAfterStep = null,
        RestTarget? restAfterSegment = null)
    {
        return new WorkoutBlockSegment(
            id,
            workoutBlockId,
            name,
            sequence,
            workMode,
            intent,
            scoreType,
            timeConstraint,
            intervalPrescription,
            estimatedSegmentDuration,
            restAfterStep,
            restAfterSegment);
    }
    
    public static WorkoutBlockSegment ForTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TargetIntensity? targetIntensity = null,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.ForTime,
            WorkIntentPrescription.ForTime(targetIntensity),
            WorkoutScoreType.TimeToComplete,
            timeConstraint: null,
            intervalPrescription: null,
            estimatedDuration,
            restAfterStep,
            restAfterSegment);
    }

    public static WorkoutBlockSegment ForTimeWithCap(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan cap,
        TargetIntensity? targetIntensity = null,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.ForTimeWithCap,
            WorkIntentPrescription.ForTime(targetIntensity),
            WorkoutScoreType.TimeToComplete,
            TimeConstraint.Cap(cap),
            intervalPrescription: null,
            estimatedDuration,
            restAfterStep,
            restAfterSegment);
    }

    public static WorkoutBlockSegment Amrap(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkIntent intent = WorkIntent.MaxEffort,
        TargetIntensity? targetIntensity = null,
        WorkoutScoreType scoreType = WorkoutScoreType.CompletedRoundsAndExtraReps,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.MaxWorkInTime,
            WorkIntentPrescription.New(intent, targetIntensity),
            scoreType,
            TimeConstraint.Window(duration),
            intervalPrescription: null,
            estimatedDuration,
            restAfterStep,
            restAfterSegment);
    }

    public static WorkoutBlockSegment MaxCaloriesInTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkIntent intent = WorkIntent.MaxEffort,
        TargetIntensity? targetIntensity = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return MaxWorkInTime(
            workoutBlockId,
            name,
            sequence,
            duration,
            WorkoutScoreType.Calories,
            intent,
            targetIntensity,
            restAfterSegment,
            restAfterStep);
    }

    public static WorkoutBlockSegment MaxDistanceInTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkIntent intent = WorkIntent.MaxEffort,
        TargetIntensity? targetIntensity = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return MaxWorkInTime(
            workoutBlockId,
            name,
            sequence,
            duration,
            WorkoutScoreType.Distance,
            intent,
            targetIntensity,
            restAfterSegment,
            restAfterStep);
    }

    public static WorkoutBlockSegment MaxRepsInTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkIntent intent = WorkIntent.MaxEffort,
        TargetIntensity? targetIntensity = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return MaxWorkInTime(
            workoutBlockId,
            name,
            sequence,
            duration,
            WorkoutScoreType.TotalReps,
            intent,
            targetIntensity,
            restAfterSegment,
            restAfterStep);
    }

    public static WorkoutBlockSegment MaxWorkInTime(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        TimeSpan duration,
        WorkoutScoreType scoreType,
        WorkIntent intent = WorkIntent.MaxEffort,
        TargetIntensity? targetIntensity = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.MaxWorkInTime,
            WorkIntentPrescription.New(intent, targetIntensity),
            scoreType,
            TimeConstraint.Window(duration),
            intervalPrescription: null,
            estimatedSegmentDuration: null,
            restAfterStep,
            restAfterSegment);
    }

    public static WorkoutBlockSegment ForQuality(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkMode workMode = WorkMode.FixedWork,
        WorkoutScoreType scoreType = WorkoutScoreType.Quality,
        TargetIntensity? targetIntensity = null,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            workMode,
            WorkIntentPrescription.ForQuality(targetIntensity),
            scoreType,
            timeConstraint: null,
            intervalPrescription: null,
            estimatedDuration,
            restAfterStep,
            restAfterSegment);
    }

    public static WorkoutBlockSegment FixedWork(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkIntent intent,
        TargetIntensity? targetIntensity = null,
        WorkoutScoreType scoreType = WorkoutScoreType.Completed,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null,
        RestTarget? restAfterStep = null)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.FixedWork,
            WorkIntentPrescription.New(intent, targetIntensity),
            scoreType,
            timeConstraint: null,
            intervalPrescription: null,
            estimatedDuration,
            restAfterStep,
            restAfterSegment);
    }

    public static WorkoutBlockSegment StepIntervals(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkIntent intent,
        TargetIntensity? targetIntensity,
        int stepIntervalSeconds,
        RestTarget? restAfterStepInterval = null,
        WorkoutScoreType scoreType = WorkoutScoreType.Completed,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null)
    {
        if (stepIntervalSeconds <= 0)
        {
            throw new ArgumentException(
                "Step interval seconds must be greater than zero.",
                nameof(stepIntervalSeconds));
        }

        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.Intervals,
            WorkIntentPrescription.New(intent, targetIntensity),
            scoreType,
            timeConstraint: null,
            intervalPrescription: NewIntervalPrescription(
                TimeSpan.FromSeconds(stepIntervalSeconds),
                IntervalScope.PerStep),
            estimatedDuration,
            restAfterStepInterval,
            restAfterSegment);
    }

    public static WorkoutBlockSegment EveryXOnTheX(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkIntent intent,
        TargetIntensity? targetIntensity,
        TimeSpan interval,
        IntervalScope scope = IntervalScope.PerStep,
        RestTarget? restAfterStepInterval = null,
        WorkoutScoreType scoreType = WorkoutScoreType.Completed,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null,
        bool startsOnClock = true)
    {
        return new WorkoutBlockSegment(
            WorkoutBlockSegmentId.New(),
            workoutBlockId,
            name,
            sequence,
            WorkMode.Intervals,
            WorkIntentPrescription.New(intent, targetIntensity),
            scoreType,
            timeConstraint: null,
            intervalPrescription: NewIntervalPrescription(
                interval,
                scope,
                startsOnClock),
            estimatedDuration,
            restAfterStepInterval,
            restAfterSegment);
    }

    public static WorkoutBlockSegment EveryXForBlockRepeat(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkIntent intent,
        TargetIntensity? targetIntensity,
        TimeSpan interval,
        WorkoutScoreType scoreType = WorkoutScoreType.Completed,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null,
        bool startsOnClock = true)
    {
        return EveryXOnTheX(
            workoutBlockId,
            name,
            sequence,
            intent,
            targetIntensity,
            interval,
            scope: IntervalScope.PerBlockRepeat,
            restAfterStepInterval: null,
            scoreType,
            estimatedDuration,
            restAfterSegment,
            startsOnClock);
    }

    public static WorkoutBlockSegment Emom(
        WorkoutBlockId workoutBlockId,
        string name,
        int sequence,
        WorkIntent intent,
        TargetIntensity? targetIntensity,
        WorkoutScoreType scoreType = WorkoutScoreType.Completed,
        EstimatedDuration? estimatedDuration = null,
        RestTarget? restAfterSegment = null)
    {
        return EveryXOnTheX(
            workoutBlockId,
            name,
            sequence,
            intent,
            targetIntensity,
            TimeSpan.FromMinutes(1),
            scope: IntervalScope.PerStep,
            restAfterStepInterval: null,
            scoreType,
            estimatedDuration,
            restAfterSegment);
    }

    public void AddStep(WorkoutBlockSegmentStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        var updatedStep = step.ApplyDefaultRestAfterStep(RestAfterStep);

        steps.Add(updatedStep);

        ResequenceSteps();
    }

    public void AddStepAt(
        int atStepSequence,
        WorkoutBlockSegmentStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        ValidateStepSequenceOrThrow(atStepSequence);

        var updatedStep = step
            .ApplyDefaultRestAfterStep(RestAfterStep)
            .WithSequence(atStepSequence);

        ShiftStepsFromSequence(atStepSequence);

        steps.Add(updatedStep);

        ResequenceSteps();
    }

    public void AddFirst(WorkoutBlockSegmentStep step)
    {
        AddStepAt(1, step);
    }

    public void AddLast(WorkoutBlockSegmentStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        var updatedStep = step
            .ApplyDefaultRestAfterStep(RestAfterStep)
            .WithSequence(GetNextStepSequence());

        steps.Add(updatedStep);

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

    private static IntervalPrescription NewIntervalPrescription(
        TimeSpan duration,
        IntervalScope scope,
        bool startsOnClock = true)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Interval duration must be greater than zero.", nameof(duration));
        }

        return new IntervalPrescription
        {
            Duration = duration,
            Scope = scope,
            StartsOnClock = startsOnClock
        };
    }

    private static void ValidateIntervalPrescription(
        WorkMode workMode,
        IntervalPrescription? intervalPrescription)
    {
        if (workMode == WorkMode.Intervals && intervalPrescription is null)
        {
            throw new ArgumentException(
                "Interval work mode requires an interval prescription.",
                nameof(intervalPrescription));
        }

        if (workMode != WorkMode.Intervals && intervalPrescription is not null)
        {
            throw new ArgumentException(
                "Interval prescription should only be set for interval work mode.",
                nameof(intervalPrescription));
        }
    }

    private int GetNextStepSequence()
    {
        if (steps.Count == 0)
        {
            return 1;
        }

        return steps.Max(step => step.Sequence) + 1;
    }

    private static void ValidateStepSequenceOrThrow(int sequence)
    {
        if (sequence < 1)
        {
            throw new ArgumentException("Step sequence must be greater than zero.", nameof(sequence));
        }
    }

    private void ShiftStepsFromSequence(int sequence)
    {
        foreach (var step in steps
                     .Where(existingStep => existingStep.Sequence >= sequence)
                     .OrderByDescending(existingStep => existingStep.Sequence)
                     .ToList())
        {
            var index = steps.IndexOf(step);
            steps[index] = step.WithSequence(step.Sequence + 1);
        }
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