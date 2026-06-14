using PRLab.API.DTO.Description;
using PRLab.API.DTO.Exercise.Relation;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Exercise;

public sealed record ExerciseGetDTO(
    ExerciseId Id,
    string Name,
    DescriptionGetDTO? Description,
    IReadOnlyList<ExerciseStepGetDTO> Blocks);