using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Muscle;

public record MuscleSummaryDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    BodySection BodySection);