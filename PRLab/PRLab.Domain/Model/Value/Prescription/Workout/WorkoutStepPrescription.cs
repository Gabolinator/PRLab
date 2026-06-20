using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Prescription;
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
    
    // todo hook
    public LateralityExecution? LateralityExecution { get; init; }

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
            Notes = notes
        };
    }

    public static WorkoutStepPrescription FromExercise(Exercise exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        var firstBlock = exercise.Steps.FirstOrDefault();

        return new WorkoutStepPrescription
        {
            WorkTarget = firstBlock?.Target,
            LoadTarget = firstBlock?.LoadTarget,
            RestAfterStep = firstBlock?.TransitionAfterStep,
            Notes = null
        };
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
}