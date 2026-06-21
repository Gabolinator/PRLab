using PRLab.Domain.Model.Join;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Structure;

public sealed record WorkoutBlockAssignmentSeedJsonDto
{
    public Guid? Id { get; init; }

    public int Sequence { get; init; }

    public WorkoutBlockSeedJsonDto Block { get; init; } = new();

    public static WorkoutBlockAssignmentSeedJsonDto FromAssignment(
        WorkoutBlockAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        return new WorkoutBlockAssignmentSeedJsonDto
        {
            Id = assignment.Id.Value,
            Sequence = assignment.Sequence,
            Block = WorkoutBlockSeedJsonDto.FromWorkoutBlock(assignment.WorkoutBlock)
        };
    }
}