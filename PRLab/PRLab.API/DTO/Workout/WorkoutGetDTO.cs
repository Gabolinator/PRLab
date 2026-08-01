using PRLab.API.DTO.Description;
using PRLab.API.DTO.Workout.WorkoutBlockAssignment;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription.Common;

namespace PRLab.API.DTO.Workout;

public sealed record WorkoutGetDTO(
    WorkoutId Id,
    string Name,
    DescriptionGetDTO? Description,
    EstimatedDuration? EstimatedDuration,
    IReadOnlyList<WorkoutBlockAssignmentGetDTO> Blocks,
    VisibilityScope Visibility);