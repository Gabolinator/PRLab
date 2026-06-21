using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Structure;

public sealed record WorkoutBlockSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public WorkoutBlockType BlockType { get; init; }

    public BlockRepeatPrescriptionSeedJsonDto BlockRepeatPrescription { get; init; } = new();

    public IReadOnlyList<WorkoutBlockSegmentSeedJsonDto> Segments { get; init; } = [];

    public DataOrigin Origin { get; init; } = DataOrigin.BuiltIn;

    public Guid? OwnerUserId { get; init; }

    public static WorkoutBlockSeedJsonDto FromWorkoutBlock(WorkoutBlock workoutBlock)
    {
        ArgumentNullException.ThrowIfNull(workoutBlock);

        return new WorkoutBlockSeedJsonDto
        {
            Id = workoutBlock.Id.Value,
            Name = workoutBlock.Name,
            NameKey = workoutBlock.NameKey,
            BlockType = workoutBlock.BlockType,
            BlockRepeatPrescription =
                BlockRepeatPrescriptionSeedJsonDto.FromBlockRepeatPrescription(
                    workoutBlock.BlockRepeatPrescription),
            Segments = workoutBlock.Segments
                .OrderBy(segment => segment.Sequence)
                .Select(WorkoutBlockSegmentSeedJsonDto.FromSegment)
                .ToList(),
            Origin = workoutBlock.Ownership.Origin,
            OwnerUserId = workoutBlock.Ownership.OwnerUserId?.Value
        };
    }
}