using PRLab.API.DTO.Description;
using PRLab.API.DTO.Exercise.Relation;

namespace PRLab.API.DTO.Exercise;

public sealed record ExercisePostDTO
{
    public required string Name { get; init; }

    public DescriptionPostDTO? Descriptor { get; init; }

    public IReadOnlyList<ExerciseBlockPostDTO> Blocks { get; init; } = [];
}