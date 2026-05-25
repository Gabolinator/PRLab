using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Join;

public sealed record DescriptionTranslation
{
    public DescriptionTranslationId Id { get; init; }

    public DescriptionId DescriptionId { get; private set; }

    public string LanguageCode { get; private set; } = string.Empty;

    public string? Content { get; private set; }

    private DescriptionTranslation()
    {
        // EF Core
    }

    private DescriptionTranslation(
        DescriptionTranslationId id,
        DescriptionId descriptionId,
        string languageCode,
        string? content)
    {
        Id = id;
        DescriptionId = descriptionId;
        LanguageCode = languageCode;
        Content = content;
    }

    public static DescriptionTranslation New(
        DescriptionId descriptionId,
        string languageCode,
        string? content)
    {
        return new DescriptionTranslation(
            DescriptionTranslationId.New(),
            descriptionId,
            languageCode,
            content
        );
    }

    public void ChangeContent(string? content)
    {
        Content = content;
    }
}