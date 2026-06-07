using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Join;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

public sealed record DescriptionTranslationSeedJsonDto
{
    public string LanguageCode { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;
    
    public SeedAction Action { get; init; } = SeedAction.Ignore;

    public static DescriptionTranslationSeedJsonDto FromTranslation(
        DescriptionTranslation translation)
    {
        ArgumentNullException.ThrowIfNull(translation);

        return new DescriptionTranslationSeedJsonDto
        {
            LanguageCode = translation.LanguageCode,
            Content = translation.Content ?? string.Empty,
        };
    }
}