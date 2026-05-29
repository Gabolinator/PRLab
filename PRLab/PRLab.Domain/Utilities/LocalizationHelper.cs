namespace PRLab.Domain.Utilities;

public static class LocalizationHelper
{
    public static Language DefaultLanguage => Language.EN;

    public enum Language
    {
        EN,
        FR,
        DE,
        IT,
        ES,
    }

    private static HashSet<Language> SupportedLanguages => Enum
        .GetValues<Language>()
        .ToHashSet();

    public static Language ValidateLanguageOrDefault(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultLanguage;
        }

        var normalizedLanguageCode = FormatingUtilities.NormalizeLanguageCode(languageCode);

        return TryParseLanguageOrDefault(normalizedLanguageCode, out var result)
            && IsValidLanguage(result)
                ? result
                : DefaultLanguage;
    }

    public static Language ValidateLanguageOrDefault(Language? language)
    {
        if (language is null)
        {
            return DefaultLanguage;
        }

        return IsValidLanguage(language.Value)
            ? language.Value
            : DefaultLanguage;
    }

    public static string ToLanguageCode(string? languageCode)
    {
        return ToLanguageCode(ValidateLanguageOrDefault(languageCode));
    }

    public static bool TryParseLanguageOrDefault(string? languageCode, out Language result)
    {
        result = DefaultLanguage;

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        var normalizedLanguageCode = FormatingUtilities.NormalizeLanguageCode(languageCode);

        return Enum.TryParse(normalizedLanguageCode, true, out result);
    }

    private static bool IsValidLanguage(Language language)
    {
        return SupportedLanguages.Contains(language);
    }
    
    public static Language ToLanguageOrDefault(string? languageCode)
    {
        return ValidateLanguageOrDefault(languageCode);
    }

    public static Language ToLanguageOrDefault(Language? language)
    {
        return ValidateLanguageOrDefault(language);
    }
    
    public static string ToLanguageCode(Language language)
    {
        return language.ToString().ToLowerInvariant();
    }

    public static string ToLanguageCodeOrDefault(Language? language)
    {
        return ToLanguageCode(ValidateLanguageOrDefault(language));
    }
}