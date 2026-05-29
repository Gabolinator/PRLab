using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;

public record DescriptionGetDTO
{
    public DescriptionId Id { get; init; }

    public LocalizationHelper.Language? RequestedLanguage { get; init; }

    public LocalizationHelper.Language ResolvedLanguage { get; init; }

    public string Content { get; init; } = string.Empty;
}