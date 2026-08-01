using PRLab.API.DTO.Description;
using PRLab.API.DTO.Muscle.Relation;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Muscle;

public sealed record MuscleGetDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    BodySection BodySection,
    DescriptionGetDTO? Description,
    IReadOnlyList<MuscleFunctionGetDTO> Functions,
    IReadOnlyList<MuscleSummaryDTO> Antagonists);