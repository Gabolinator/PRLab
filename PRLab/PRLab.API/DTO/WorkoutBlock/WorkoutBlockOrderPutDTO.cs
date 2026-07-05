namespace PRLab.API.DTO.WorkoutBlock;

public sealed record WorkoutBlockOrderPutDTO
{
    public required IReadOnlyList<WorkoutBlockAssignmentOrderPutDTO> Blocks { get; init; }
}