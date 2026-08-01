using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Workout;

namespace PRLab.API.DTO.Workout.WorkoutBlockSegmentStep;

using System.ComponentModel.DataAnnotations;

public sealed record WorkoutBlockSegmentStepPostDTO
{
    public required WorkoutStepKind StepKind { get; init; }

    [Range(1, int.MaxValue)]
    public required int Sequence { get; init; }

    public ExerciseId? ExerciseId { get; init; }

    public WorkoutStepPrescription? Prescription { get; init; }

    public RestTarget? Rest { get; init; }

    [StringLength(1000)]
    public string? Notes { get; init; }
}