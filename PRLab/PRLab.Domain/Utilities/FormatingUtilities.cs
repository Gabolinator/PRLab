namespace PRLab.Domain.Utilities;

public static class FormatingUtilities
{
    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return NormalizeNonNullString(name);
    }
    
    public static string NormalizeLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            throw new ArgumentException("Language code is required.");
        }

        return NormalizeNonNullString(languageCode);
    }

    public static string NormalizeLanguageCodeOrDefault(
        string? languageCode,
        string defaultLanguageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return defaultLanguageCode;
        }

        return NormalizeLanguageCode(languageCode);
    }

    public static string NormalizeNullableString(string? content)
        => !string.IsNullOrWhiteSpace(content) ?  NormalizeNonNullString(content) : string.Empty;

    public static string NormalizeNonNullString(string content)
        => content.Trim().ToLowerInvariant();

}