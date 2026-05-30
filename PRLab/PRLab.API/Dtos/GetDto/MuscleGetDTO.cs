using PRLab.API.Dtos.SummaryDto;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;

public sealed record MuscleGetDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    DomainEnum.BodySection BodySection,
    DescriptionGetDTO? Description,
    IReadOnlyList<MuscleSummaryDTO>? Antagonists);
    