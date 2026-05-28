
using PRLab.API.Dtos.GetDto;
using PRLab.API.Dtos.PostDto;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper;

public static class DescriptionMapper
{
    public static DescriptionGetDTO ToGetDTO(
        Description description,
        string? languageCode = null)
    {
        var requestedLanguage = LocalizationHelper.ValidateLanguageOrDefault(languageCode);
        var resolvedLanguage = description.GetResolvedLanguageCode(requestedLanguage);

        return new DescriptionGetDTO
        {
            Id = description.Id,
            RequestedLanguage = requestedLanguage,
            ResolvedLanguage = resolvedLanguage,
            Content = description.GetContent(requestedLanguage) ?? string.Empty,
        };
    }
    

    public static Description ToEntity(DescriptionPostDTO payload) =>
        Description.New(payload.Content, payload.Language);
    

    public static IReadOnlyList<DescriptionGetDTO> ToGetDTOs(
        IReadOnlyList<Description> descriptions,
        string? languageCode = null)
    {
        return descriptions
            .Select(description => ToGetDTO(description, languageCode))
            .ToList();
    }
    
}