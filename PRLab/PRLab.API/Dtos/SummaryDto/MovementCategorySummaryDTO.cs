using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.SummaryDto;

public record MovementCategorySummaryDTO(
    MovementCategoryId Id,
    string Name);