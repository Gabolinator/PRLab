using PRLab.API.DTO.Workout.WorkoutBlock;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Workout.WorkoutBlockAssignment;

public sealed record WorkoutBlockAssignmentGetDTO(
    WorkoutBlockAssignmentId Id,
    int Sequence,
    WorkoutBlockGetDTO Block);