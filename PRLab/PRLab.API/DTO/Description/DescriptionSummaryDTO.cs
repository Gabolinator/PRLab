using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Description;

public record DescriptionSummaryDTO(
    DescriptionId Id,
    string Content)
{
}