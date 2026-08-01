using PRLab.API.DTO.Workout.WorkoutSegments;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription.Workout;

namespace PRLab.API.DTO.Workout.WorkoutBlock;

public sealed record WorkoutBlockGetDTO(
    WorkoutBlockId Id,
    string Name,
    WorkoutBlockType BlockType,
    BlockRepeatPrescription RepeatPrescription,
    IReadOnlyList<WorkoutBlockSegmentGetDTO> Segments,
    VisibilityScope Visibility);