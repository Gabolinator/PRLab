using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;

public record DescriptionGetDTO
{
    public DescriptionId Id { get; init; }

    public string RequestedLanguage { get; init; } = string.Empty;

    public string ResolvedLanguage { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;
}