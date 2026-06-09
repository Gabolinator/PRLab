using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.MovementCategory;

public record MovementCategorySummaryDTO(
    MovementCategoryId Id,
    string Name);