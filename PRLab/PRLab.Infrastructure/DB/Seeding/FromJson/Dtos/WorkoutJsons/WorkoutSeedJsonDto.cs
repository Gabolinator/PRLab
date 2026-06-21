using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Structure;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons;

public sealed record WorkoutSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public DescriptionSeedJsonDto? Description { get; init; }

    public EstimatedDurationSeedJsonDto? EstimatedDuration { get; init; }

    public IReadOnlyList<WorkoutBlockAssignmentSeedJsonDto> Blocks { get; init; } = [];

    public DataOrigin Origin { get; init; } = DataOrigin.BuiltIn;

    public Guid? OwnerUserId { get; init; }

    public SeedAction Action { get; init; } = SeedAction.Ignore;

    public static WorkoutSeedJsonDto FromWorkout(Workout workout)
    {
        ArgumentNullException.ThrowIfNull(workout);

        return new WorkoutSeedJsonDto
        {
            Id = workout.Id.Value,
            Name = workout.Name,
            NameKey = workout.NameKey,
            Description = workout.Description is null
                ? null
                : DescriptionSeedJsonDto.FromDescription(workout.Description),
            EstimatedDuration = EstimatedDurationSeedJsonDto.FromEstimatedDuration(
                workout.EstimatedDuration),
            Blocks = workout.Blocks
                .OrderBy(assignment => assignment.Sequence)
                .Select(WorkoutBlockAssignmentSeedJsonDto.FromAssignment)
                .ToList(),
            Origin = workout.Ownership.Origin,
            OwnerUserId = workout.Ownership.OwnerUserId?.Value,
            Action = SeedAction.Ignore
        };
    }
}