using PRLab.API.DTO.Description;
using PRLab.API.DTO.Workout.WorkoutBlockAssignment;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Prescription.Common;

namespace PRLab.API.DTO.Workout;

public sealed record WorkoutPostDTO
{
    public required string Name { get; init; }

    public DescriptionPostDTO? Description { get; init; }

    public EstimatedDuration? EstimatedDuration { get; init; }

    public IReadOnlyList<WorkoutBlockAssignmentPostDTO> Blocks { get; init; } = [];

    public VisibilityScope? Visibility { get; init; }
}