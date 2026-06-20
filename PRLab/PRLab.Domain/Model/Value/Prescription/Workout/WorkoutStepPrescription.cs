using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Domain.Model.Value.Prescription.Workout;

public sealed record WorkoutStepPrescription
{
    public WorkTarget? WorkTarget { get; init; }

    public LoadTarget? LoadTarget { get; init; }

    public RestTarget? RestAfterStep { get; init; }

    public TimeConstraint? TimeConstraint { get; init; }

    public EstimatedDuration? EstimatedStepDuration { get; init; }

    public WorkIntentPrescription? IntentOverride { get; init; }

    public WorkPartitionPrescription? Partition { get; init; }

    public LateralityExecution? SideExecution { get; init; }

    public string? Notes { get; init; }

    private WorkoutStepPrescription()
    {
        // EF Core
    }

    public static WorkoutStepPrescription New(
        WorkTarget? workTarget = null,
        LoadTarget? loadTarget = null,
        RestTarget? restAfterStep = null,
        TimeConstraint? timeConstraint = null,
        EstimatedDuration? estimatedDuration = null,
        WorkPartitionPrescription? partition = null,
        WorkIntentPrescription? intentOverride = null,
        LateralityExecution? lateralityExecution = null,
        string? notes = null)
    {
        return new WorkoutStepPrescription
        {
            WorkTarget = workTarget,
            LoadTarget = loadTarget,
            RestAfterStep = restAfterStep,
            TimeConstraint = timeConstraint,
            EstimatedStepDuration = estimatedDuration,
            Partition = partition,
            IntentOverride = intentOverride,
            SideExecution = ValidateLateralityExecution(
                workTarget,
                lateralityExecution),
            Notes = notes
        };
    }

    public static WorkoutStepPrescription FromExercise(Exercise exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var firstBlock = exercise.Steps.FirstOrDefault();

        return New(
            workTarget: firstBlock?.Target,
            loadTarget: firstBlock?.LoadTarget,
            restAfterStep: firstBlock?.TransitionAfterStep);
    }

    public WorkoutStepPrescription WithEstimatedDurationSeconds(int seconds)
    {
        return this with
        {
            EstimatedStepDuration = EstimatedDuration.Seconds(seconds),
        };
    }

    public WorkoutStepPrescription WithRestAfterStep(RestTarget? restAfterStep)
    {
        return this with
        {
            RestAfterStep = restAfterStep,
        };
    }

    public WorkoutStepPrescription WithLateralityExecution(
        LateralityExecution? lateralityExecution)
    {
        return this with
        {
            SideExecution = ValidateLateralityExecution(
                WorkTarget,
                lateralityExecution)
        };
    }

    public WorkoutStepPrescription AsAlternating()
    {
        return WithLateralityExecution(LateralityExecution.Alternating);
    }

    public WorkoutStepPrescription AsLeftThenRight()
    {
        return WithLateralityExecution(LateralityExecution.LeftThenRight);
    }

    public WorkoutStepPrescription AsRightThenLeft()
    {
        return WithLateralityExecution(LateralityExecution.RightThenLeft);
    }

    public WorkoutStepPrescription AsWeakSideFirst()
    {
        return WithLateralityExecution(LateralityExecution.WeakSideFirst);
    }

    private static LateralityExecution? ValidateLateralityExecution(
        WorkTarget? workTarget,
        LateralityExecution? lateralityExecution)
    {
        if (lateralityExecution is null)
        {
            return null;
        }

        ValidateLateralityExecutionValue(lateralityExecution.Value);

        if (workTarget is null)
        {
            if (lateralityExecution == LateralityExecution.NotApplicable)
            {
                return lateralityExecution;
            }

            throw new ArgumentException(
                "Laterality execution requires a work target.",
                nameof(lateralityExecution));
        }

        return workTarget.Scope switch
        {
            WorkTargetScope.Total => ValidateForTotalScope(lateralityExecution.Value),
            WorkTargetScope.PerSide => ValidateForPerSideScope(lateralityExecution.Value),
            WorkTargetScope.Left => ValidateForLeftScope(lateralityExecution.Value),
            WorkTargetScope.Right => ValidateForRightScope(lateralityExecution.Value),
            _ => throw new ArgumentOutOfRangeException(
                nameof(workTarget.Scope),
                workTarget.Scope,
                "Unsupported work target scope.")
        };
    }

    private static void ValidateLateralityExecutionValue(
        LateralityExecution lateralityExecution)
    {
        switch (lateralityExecution)
        {
            case LateralityExecution.NotApplicable:
            case LateralityExecution.Alternating:
            case LateralityExecution.LeftThenRight:
            case LateralityExecution.RightThenLeft:
            case LateralityExecution.WeakSideFirst:
            case LateralityExecution.LeftOnly:
            case LateralityExecution.RightOnly:
                return;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(lateralityExecution),
                    lateralityExecution,
                    "Unsupported laterality execution.");
        }
    }

    private static LateralityExecution ValidateForTotalScope(
        LateralityExecution lateralityExecution)
    {
        return lateralityExecution switch
        {
            LateralityExecution.NotApplicable => lateralityExecution,

            // Example: 20 total alternating lunges.
            LateralityExecution.Alternating => lateralityExecution,

            _ => throw new ArgumentException(
                $"{lateralityExecution} requires a side-specific or per-side work target.")
        };
    }

    private static LateralityExecution ValidateForPerSideScope(
        LateralityExecution lateralityExecution)
    {
        return lateralityExecution switch
        {
            LateralityExecution.Alternating => lateralityExecution,
            LateralityExecution.LeftThenRight => lateralityExecution,
            LateralityExecution.RightThenLeft => lateralityExecution,
            LateralityExecution.WeakSideFirst => lateralityExecution,

            _ => throw new ArgumentException(
                $"{lateralityExecution} is not valid for a per-side work target.")
        };
    }

    private static LateralityExecution ValidateForLeftScope(
        LateralityExecution lateralityExecution)
    {
        return lateralityExecution switch
        {
            LateralityExecution.LeftOnly => lateralityExecution,

            _ => throw new ArgumentException(
                $"{lateralityExecution} is not valid for a left-side-only work target.")
        };
    }

    private static LateralityExecution ValidateForRightScope(
        LateralityExecution lateralityExecution)
    {
        return lateralityExecution switch
        {
            LateralityExecution.RightOnly => lateralityExecution,

            _ => throw new ArgumentException(
                $"{lateralityExecution} is not valid for a right-side-only work target.")
        };
    }
}