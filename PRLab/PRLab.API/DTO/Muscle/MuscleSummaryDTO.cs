using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Muscle;

public record MuscleSummaryDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    DomainEnum.BodySection BodySection);