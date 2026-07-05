using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.WorkoutBlock;

public sealed record WorkoutBlockAssignmentOrderPutDTO
{
    public required WorkoutBlockAssignmentId AssignmentId { get; init; }

    public required int Sequence { get; init; }
}