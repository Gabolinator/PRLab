using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.SummaryDto;

public record DescriptionSummaryDTO(
    DescriptionId Id,
    string Content)
{
}