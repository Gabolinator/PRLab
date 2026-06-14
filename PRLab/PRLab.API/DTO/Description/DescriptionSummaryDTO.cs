using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Description;

public record DescriptionSummaryDTO(
    DescriptionId Id,
    string Content)
{
}