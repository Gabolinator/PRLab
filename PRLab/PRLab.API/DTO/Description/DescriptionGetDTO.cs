using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;

namespace PRLab.API.DTO.Description;

public record DescriptionGetDTO
{
    public DescriptionId Id { get; init; }

    public LocalizationHelper.Language? RequestedLanguage { get; init; }

    public LocalizationHelper.Language ResolvedLanguage { get; init; }

    public string Content { get; init; } = string.Empty;
}