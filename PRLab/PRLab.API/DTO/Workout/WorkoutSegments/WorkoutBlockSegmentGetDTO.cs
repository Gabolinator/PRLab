using PRLab.API.DTO.Workout.WorkoutBlockSegmentStep;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.API.DTO.Workout.WorkoutSegments;

public sealed record WorkoutBlockSegmentGetDTO(
    WorkoutBlockSegmentId Id,
    string Name,
    int Sequence,
    WorkMode WorkMode,
    WorkIntentPrescription Intent,
    WorkoutScoreType ScoreType,
    TimeConstraint? TimeConstraint,
    IntervalPrescription? IntervalPrescription,
    EstimatedDuration? EstimatedSegmentDuration,
    RestTarget? RestAfterStep,
    RestTarget? RestAfterSegment,
    IReadOnlyList<WorkoutBlockSegmentStepGetDTO> Steps);