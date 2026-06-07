using PRLab.API.DTO.Description;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper;

public static class DescriptionMapper
{
    
    public static Description ToEntity(DescriptionPostDTO payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return Description.New(
            payload.Content,
            payload.Language
        );
    }
    
    public static DescriptionGetDTO ToGetDTO(
        Description description,
        string? languageCode)
    {
        LocalizationHelper.Language? requestedLanguage = string.IsNullOrWhiteSpace(languageCode)
            ? null
            : LocalizationHelper.ValidateLanguageOrDefault(languageCode);

        return ToGetDTO(description, requestedLanguage);
    }

    public static DescriptionGetDTO ToGetDTO(
        Description description,
        LocalizationHelper.Language? language = (LocalizationHelper.Language?)null)
    {
        var resolvedLanguageCode = description.GetResolvedLanguageCode(language);
        var resolvedLanguage = LocalizationHelper.ValidateLanguageOrDefault(resolvedLanguageCode);

        return new DescriptionGetDTO
        {
            Id = description.Id,
            RequestedLanguage = language,
            ResolvedLanguage = resolvedLanguage,
            Content = description.GetContent(language) ?? string.Empty,
        };
    }

    public static IReadOnlyList<DescriptionGetDTO> ToGetDTOs(
        IReadOnlyList<Description> descriptions,
        string? languageCode)
    {
        LocalizationHelper.Language? requestedLanguage = string.IsNullOrWhiteSpace(languageCode)
            ? null
            : LocalizationHelper.ValidateLanguageOrDefault(languageCode);

        return ToGetDTOs(descriptions, requestedLanguage);
    }

    public static IReadOnlyList<DescriptionGetDTO> ToGetDTOs(
        IReadOnlyList<Description> descriptions,
        LocalizationHelper.Language? language = (LocalizationHelper.Language?)null)
    {
        return descriptions
            .Select(description => ToGetDTO(description, language))
            .ToList();
    }
}