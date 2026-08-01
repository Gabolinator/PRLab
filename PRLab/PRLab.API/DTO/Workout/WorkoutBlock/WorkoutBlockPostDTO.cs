using System.ComponentModel.DataAnnotations;
using PRLab.API.DTO.Workout.WorkoutSegments;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Prescription.Workout;

namespace PRLab.API.DTO.Workout.WorkoutBlock;

public sealed record WorkoutBlockPostDTO
{
    [Required]
    [StringLength(256, MinimumLength = 2)]
    public required string Name { get; init; }

    public required WorkoutBlockType BlockType { get; init; }

    public required BlockRepeatPrescription RepeatPrescription { get; init; }

    public IReadOnlyList<WorkoutBlockSegmentPostDTO> Segments { get; init; } = [];

    public VisibilityScope? Visibility { get; init; }
}