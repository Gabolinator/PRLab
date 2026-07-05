using PRLab.API.DTO.Description;
using PRLab.API.DTO.Exercise.Relation;
using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.API.DTO.Exercise;

public sealed record ExercisePostDTO
{
    public required string Name { get; init; }

    public DescriptionPostDTO? Descriptor { get; init; }

    public IReadOnlyList<ExerciseStepPostDTO> Steps { get; init; } = [];
    
    public required VisibilityScope Visibility { get; init; }
}