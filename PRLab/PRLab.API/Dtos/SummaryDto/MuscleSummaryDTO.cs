using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.SummaryDto;

public record MuscleSummaryDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    DomainEnum.BodySection BodySection);