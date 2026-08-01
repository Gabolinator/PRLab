using PRLab.API.DTO.Exercise;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Workout;

namespace PRLab.API.DTO.Workout.WorkoutBlockSegmentStep;

public sealed record WorkoutBlockSegmentStepGetDTO(
    WorkoutBlockSegmentStepId Id,
    WorkoutStepKind StepKind,
    int Sequence,
    ExerciseGetDTO? Exercise,
    WorkoutStepPrescription? Prescription,
    RestTarget? Rest,
    string? Notes);