using PRLab.API.DTO.Workout.WorkoutBlock;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Workout.WorkoutBlockAssignment;

public sealed record WorkoutBlockAssignmentPostDTO
{
    public required WorkoutBlockId WorkoutBlockId { get; init; }
    
    public required WorkoutBlockPostDTO Block { get; init; }

    public int? Sequence { get; init; }
}