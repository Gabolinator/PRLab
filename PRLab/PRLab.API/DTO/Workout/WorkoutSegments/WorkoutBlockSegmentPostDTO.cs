using System.ComponentModel.DataAnnotations;
using PRLab.API.DTO.Workout.WorkoutBlockSegmentStep;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.API.DTO.Workout.WorkoutSegments;

public class WorkoutBlockSegmentPostDTO
{
    [Required]
    [StringLength(256, MinimumLength = 2)]
    public required string Name { get; init; }

    [Range(1, int.MaxValue)]
    public required int Sequence { get; init; }

    public required WorkMode WorkMode { get; init; }

    public required WorkIntentPrescription Intent { get; init; }

    public required WorkoutScoreType ScoreType { get; init; }

    public TimeConstraint? TimeConstraint { get; init; }

    public IntervalPrescription? IntervalPrescription { get; init; }

    public EstimatedDuration? EstimatedSegmentDuration { get; init; }

    public RestTarget? RestAfterStep { get; init; }

    public RestTarget? RestAfterSegment { get; init; }

    public IReadOnlyList<WorkoutBlockSegmentStepPostDTO> Steps { get; init; } = [];
}