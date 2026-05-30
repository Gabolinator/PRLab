using System.Text.RegularExpressions;

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

    public static string NormalizeNameKey(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        var normalizedName = NormalizeNonNullString(name);

        return Regex.Replace(
            normalizedName,
            @"[\s\-_]+",
            string.Empty);
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

    public static string LanguageCodeToString(
        LocalizationHelper.Language? languageCode,
        LocalizationHelper.Language defaultLanguageCode)
    {
        return languageCode?.ToString().ToLowerInvariant() ??
               defaultLanguageCode.ToString().ToLowerInvariant();
    }

    public static string NormalizeNullableString(string? content)
    {
        return !string.IsNullOrWhiteSpace(content)
            ? NormalizeNonNullString(content)
            : string.Empty;
    }

    public static string NormalizeNonNullString(string content)
    {
        return Regex.Replace(
                content.Trim(),
                @"\s+",
                " ")
            .ToLowerInvariant();
    }

    public static string? NormalizeDescriptionContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        { 
            return null;
        }
        
        return content.Trim();
    }
}