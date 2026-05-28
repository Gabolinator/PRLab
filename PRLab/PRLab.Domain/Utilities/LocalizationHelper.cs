namespace PRLab.Domain.Utilities;

public static class LocalizationHelper
{
    public static string DefaultLanguage => "en";

    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "en",
        "fr",
        "de",
        "it",
        "es"
    };

    public static string ValidateLanguageOrDefault(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultLanguage;
        }

        var normalizedLanguageCode = FormatingUtilities.NormalizeLanguageCode(languageCode);

        return IsValidLanguage(normalizedLanguageCode)
            ? normalizedLanguageCode
            : DefaultLanguage;
    }

    private static bool IsValidLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        var normalizedLanguageCode = FormatingUtilities.NormalizeLanguageCode(languageCode);

        return SupportedLanguages.Contains(normalizedLanguageCode);
    }
}