using PRLab.API.DTO.Workout.WorkoutBlockAssignment;

namespace PRLab.API.DTO.Workout.WorkoutBlock;

public sealed record WorkoutBlockOrderPutDTO
{
    public required IReadOnlyList<WorkoutBlockAssignmentOrderPutDTO> Blocks { get; init; }
}