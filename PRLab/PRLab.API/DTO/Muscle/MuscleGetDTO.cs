using PRLab.API.DTO.Description;
using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Muscle;

public sealed record MuscleGetDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    BodySection BodySection,
    DescriptionGetDTO? Description,
    IReadOnlyList<MuscleSummaryDTO>? Antagonists);
    