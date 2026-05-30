using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Entity;

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
        DefaultLanguageCode = LocalizationHelper.ToLanguageCode(defaultLanguageCode);
    }

    private Description(
        DescriptionId id,
        LocalizationHelper.Language language)
        : this(
            id,
            LocalizationHelper.ToLanguageCode(language))
    {
    }

    public static Description New(
        string? content,
        string? languageCode = null)
    {
        var language = LocalizationHelper.ToLanguageCode(languageCode);

        var description = new Description(
            DescriptionId.New(),
            language
        );

        description.ChangeContent(
            content,
            language
        );

        return description;
    }

    public static Description New(
        string? content,
        LocalizationHelper.Language? language)
    {
        var languageCode = LocalizationHelper.ToLanguageCodeOrDefault(language);

        return New(
            content,
            languageCode
        );
    }

    public string? GetContent(string? languageCode = null)
    {
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCode(languageCode);

        return GetContentByLanguageCode(normalizedLanguageCode);
    }

    public string? GetContent(LocalizationHelper.Language? language)
    {
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCodeOrDefault(language);

        return GetContentByLanguageCode(normalizedLanguageCode);
    }

    public string GetResolvedLanguageCode(string? languageCode = null)
    {
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCode(languageCode);

        return GetResolvedLanguageCodeByLanguageCode(normalizedLanguageCode);
    }

    public string GetResolvedLanguageCode(LocalizationHelper.Language? language)
    {
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCodeOrDefault(language);

        return GetResolvedLanguageCodeByLanguageCode(normalizedLanguageCode);
    }

    public Description ChangeContent(
        string? content,
        string? languageCode = null)
    {
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCode(languageCode);

        return ChangeContentByLanguageCode(
            content,
            normalizedLanguageCode
        );
    }

    public Description ChangeContent(
        string? content,
        LocalizationHelper.Language? language)
    {
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCodeOrDefault(language);

        return ChangeContentByLanguageCode(
            content,
            normalizedLanguageCode
        );
    }

    public Description RemoveContent(string? languageCode = null)
    {
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCode(languageCode);

        return RemoveContentByLanguageCode(normalizedLanguageCode);
    }

    public Description RemoveContent(LocalizationHelper.Language? language)
    {
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCodeOrDefault(language);

        return RemoveContentByLanguageCode(normalizedLanguageCode);
    }

    public void ChangeDefaultLanguage(string? languageCode)
    {
        DefaultLanguageCode = LocalizationHelper.ToLanguageCode(languageCode);
    }

    public void ChangeDefaultLanguage(LocalizationHelper.Language? language)
    {
        DefaultLanguageCode = LocalizationHelper.ToLanguageCodeOrDefault(language);
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

    private string? GetContentByLanguageCode(string languageCode)
    {
        var translation = translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);

        if (translation is not null)
        {
            return translation.Content;
        }

        var defaultTranslation = translations
            .FirstOrDefault(translation => translation.LanguageCode == DefaultLanguageCode);

        return defaultTranslation?.Content;
    }

    private string GetResolvedLanguageCodeByLanguageCode(string languageCode)
    {
        var translation = translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);

        if (translation is not null)
        {
            return translation.LanguageCode;
        }

        var defaultTranslation = translations
            .FirstOrDefault(translation => translation.LanguageCode == DefaultLanguageCode);

        return defaultTranslation?.LanguageCode ?? DefaultLanguageCode;
    }

    private Description ChangeContentByLanguageCode(
        string? content,
        string languageCode)
    {
        var normalizedContent = FormatingUtilities.NormalizeDescriptionContent(content);

        var translation = translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);

        if (translation is null)
        {
            translations.Add(
                DescriptionTranslation.New(
                    Id,
                    languageCode,
                    normalizedContent
                )
            );

            return this;
        }

        translation.ChangeContent(normalizedContent);

        return this;
    }

    private Description RemoveContentByLanguageCode(string languageCode)
    {
        var translation = translations
            .FirstOrDefault(translation => translation.LanguageCode == languageCode);

        if (translation is null)
        {
            return this;
        }

        translations.Remove(translation);

        return this;
    }
}