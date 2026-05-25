using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;

public sealed record Description
{
    public DescriptionId Id { get; init; }

    public string DefaultLanguageCode { get; private set; } = string.Empty;

    private readonly List<DescriptionTranslation> translations = [];

    public IReadOnlyCollection<DescriptionTranslation> Translations => translations;

    private HashSet<string> LanguageCodes => Translations
        .Select(translation => translation.LanguageCode)
        .ToHashSet();

    private Description()
    {
        // EF Core
    }

    private Description(
        DescriptionId id,
        string defaultLanguageCode)
    {
        Id = id;
        DefaultLanguageCode = FormatingUtilities.NormalizeLanguageCode(defaultLanguageCode);
    }

    public static Description New(
        string? content,
        string languageCode = "en")
    {
        var description = new Description(
            DescriptionId.New(),
            languageCode
        );

        description.ChangeContent(
            content,
            languageCode
        );

        return description;
    }

    public string? GetContent(string? languageCode = null)
    {
        var normalizedLanguageCode = FormatingUtilities.NormalizeLanguageCodeOrDefault(
            languageCode,
            DefaultLanguageCode
        );

        var translation = translations
            .FirstOrDefault(translation => translation.LanguageCode == normalizedLanguageCode);

        if (translation is not null)
        {
            return translation.Content;
        }

        var defaultTranslation = translations
            .FirstOrDefault(translation => translation.LanguageCode == DefaultLanguageCode);

        return defaultTranslation?.Content;
    }

    public Description ChangeContent(
        string? content,
        string languageCode = "en")
    {
        var normalizedLanguageCode = FormatingUtilities.NormalizeLanguageCode(languageCode);
        var normalizedContent = NormalizeContent(content);

        var translation = translations
            .FirstOrDefault(translation => translation.LanguageCode == normalizedLanguageCode);

        if (translation is null)
        {
            translations.Add(
                DescriptionTranslation.New(
                    Id,
                    normalizedLanguageCode,
                    normalizedContent
                )
            );

            return this;
        }

        translation.ChangeContent(normalizedContent);

        return this;
    }

    public Description RemoveContent(string languageCode = "en")
    {
        var normalizedLanguageCode = FormatingUtilities.NormalizeLanguageCode(languageCode);

        var translation = translations
            .FirstOrDefault(translation => translation.LanguageCode == normalizedLanguageCode);

        if (translation is null)
        {
            return this;
        }

        translations.Remove(translation);

        return this;
    }

    public void ChangeDefaultLanguage(string languageCode)
    {
        DefaultLanguageCode = FormatingUtilities.NormalizeLanguageCode(languageCode);
    }

    private static string? NormalizeContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        return content.Trim();
    }

    public Description Copy()
    {
        var description = new Description(
            DescriptionId.New(),
            DefaultLanguageCode
        );

        foreach (var translation in Translations)
        {
            description.ChangeContent(
                translation.Content,
                translation.LanguageCode
            );
        }

        return description;
    }
}