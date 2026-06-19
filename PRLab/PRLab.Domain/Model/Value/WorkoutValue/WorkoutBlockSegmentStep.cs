using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription;

namespace PRLab.Domain.Model.Value.WorkoutValue;

public sealed record WorkoutBlockSegmentStep
{
    public WorkoutBlockSegmentStepId Id { get; init; }

    public WorkoutBlockSegmentId SegmentId { get; init; }

    public ExerciseId? ExerciseId { get; init; }

    public Exercise? Exercise { get; init; }

    public WorkoutStepKind StepKind { get; init; }

    public WorkoutStepPrescription? Prescription { get; init; }

    public int Sequence { get; init; }

    public RestTarget? Rest { get; init; } // if a rest step

    public string? Notes { get; init; }

    private WorkoutBlockSegmentStep()
    {
        // EF Core
    }

    private WorkoutBlockSegmentStep(
        WorkoutBlockSegmentStepId id,
        WorkoutBlockSegmentId segmentId,
        ExerciseId? exerciseId,
        Exercise? exercise,
        WorkoutStepKind stepKind,
        WorkoutStepPrescription? prescription,
        int sequence,
        RestTarget? rest,
        string? notes)
    {
        if (StepKind == WorkoutStepKind.Rest && (Rest is null || Rest.IsEmpty()))
        {
            throw new ArgumentException("Workout Rest block must have RestTarget");
        }
        
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout block segment step id cannot be empty.", nameof(id));
        }

        if (segmentId.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout block segment id cannot be empty.", nameof(segmentId));
        }

        ValidateSequenceOrThrow(sequence);

        Id = id;
        SegmentId = segmentId;
        ExerciseId = exerciseId;
        Exercise = exercise;
        StepKind = stepKind;
        Prescription = prescription;
        Sequence = sequence;
        Rest = rest;
        Notes = notes;

        ValidateOrThrow();
    }

    public static WorkoutBlockSegmentStep NewExerciseStep(
        WorkoutBlockSegmentId segmentId,
        Exercise exercise,
        int sequence,
        WorkoutStepPrescription? prescription = null,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        return new WorkoutBlockSegmentStep(
            WorkoutBlockSegmentStepId.New(),
            segmentId,
            exercise.Id,
            exercise,
            WorkoutStepKind.Exercise,
            prescription ?? WorkoutStepPrescription.FromExercise(exercise),
            sequence,
            null,
            notes ?? prescription?.Notes);
    }

    public static WorkoutBlockSegmentStep NewRestStep(
        WorkoutBlockSegmentId segmentId,
        RestTarget rest,
        int sequence,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(rest);

        return new WorkoutBlockSegmentStep(
            WorkoutBlockSegmentStepId.New(),
            segmentId,
            null,
            null,
            WorkoutStepKind.Rest,
            null,
            sequence,
            rest,
            notes);
    }

    public static WorkoutBlockSegmentStep NewInstructionStep(
        WorkoutBlockSegmentId segmentId,
        string notes,
        int sequence)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            throw new ArgumentException("Instruction step notes cannot be empty.", nameof(notes));
        }

        return new WorkoutBlockSegmentStep(
            WorkoutBlockSegmentStepId.New(),
            segmentId,
            null,
            null,
            WorkoutStepKind.Instruction,
            null,
            sequence,
            null,
            notes);
    }

    public bool IsValid()
    {
        if (StepKind == WorkoutStepKind.Rest && Rest is null)
        {
            return false;
        }

        if (StepKind == WorkoutStepKind.Exercise && ExerciseId is null)
        {
            return false;
        }

        if (StepKind == WorkoutStepKind.Exercise && Prescription is null)
        {
            return false;
        }

        if (StepKind == WorkoutStepKind.Instruction && string.IsNullOrWhiteSpace(Notes))
        {
            return false;
        }

        return true;
    }
    
    public WorkoutBlockSegmentStep WithRestAfterStep(RestTarget? restAfterStep)
    {
        var prescription = Prescription ?? WorkoutStepPrescription.New();

        return this with
        {
            Prescription = prescription.WithRestAfterStep(restAfterStep)
        };
    }
    
    public WorkoutBlockSegmentStep WithSequence(int sequence)
    {
       ValidateSequenceOrThrow(sequence);
       return this with
       {
           Sequence = sequence,
       };
    }

    public WorkoutBlockSegmentStep ApplyDefaultRestAfterStep(RestTarget? defaultRestAfterStep)
    {
        if (StepKind != WorkoutStepKind.Exercise)
        {
            return this;
        }

        if (defaultRestAfterStep is null)
        {
            return this;
        }

        if (Prescription?.RestAfterStep is not null)
        {
            return this;
        }

        return WithRestAfterStep(defaultRestAfterStep);
    }
    
    private void ValidateOrThrow()
    {
        if (!IsValid())
        {
            throw new InvalidOperationException("Workout block segment step is invalid.");
        }
    }
    
    private static void ValidateSequenceOrThrow(int sequence)
    {
        if (sequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.", nameof(sequence));
        }
    }
}