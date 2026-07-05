using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.WorkoutBlock;

public sealed record WorkoutBlockAssignmentPostDTO
{
    public required WorkoutBlockId WorkoutBlockId { get; init; }

    public int? Sequence { get; init; }
}