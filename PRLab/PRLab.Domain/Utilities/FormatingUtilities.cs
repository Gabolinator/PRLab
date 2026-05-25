namespace PRLab.Domain.Utilities;

public static class FormatingUtilities
{
    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
            
        return name.Trim();
    }
    
    public static string NormalizeLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            throw new ArgumentException("Language code is required.");
        }

        return languageCode.Trim().ToLowerInvariant();
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
}