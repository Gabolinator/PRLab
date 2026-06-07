using PRLab.API.DTO.Description;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Muscle;

public sealed record MuscleGetDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    DomainEnum.BodySection BodySection,
    DescriptionGetDTO? Description,
    IReadOnlyList<MuscleSummaryDTO>? Antagonists);
    