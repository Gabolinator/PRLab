using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Muscle;

public record MovementCategorySummaryDTO(
    MovementCategoryId Id,
    string Name);