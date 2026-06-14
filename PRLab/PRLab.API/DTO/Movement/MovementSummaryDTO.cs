using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Movement;

public record MovementSummaryDTO(
    MovementId Id,
    string Name);