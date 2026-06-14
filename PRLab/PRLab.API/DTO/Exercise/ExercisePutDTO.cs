using PRLab.API.DTO.Description;
using PRLab.API.DTO.Exercise.Relation;

namespace PRLab.API.DTO.Exercise;

public sealed record ExercisePutDTO
{
    public required string Name { get; init; }

    public DescriptionPutDTO? Description { get; init; }

    public IReadOnlyList<ExerciseStepPutDTO> Blocks { get; init; } = [];
}