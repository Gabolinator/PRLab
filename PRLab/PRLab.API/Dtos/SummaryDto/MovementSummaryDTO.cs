using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.SummaryDto;

public record MovementSummaryDTO(
    MovementId Id,
    string Name,
    MovementCategoryId? CategoryId,
    string? CategoryName,
    MovementId? VariantOfMovementId);