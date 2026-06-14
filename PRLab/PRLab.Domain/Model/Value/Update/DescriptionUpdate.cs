using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Update;

public sealed record DescriptionUpdate
{
    public string? Content { get; init; }

    public LocalizationHelper.Language? Language { get; init; }

    public User? UpdatedBy { get; init; }

    public static DescriptionUpdate New(
        string? content,
        LocalizationHelper.Language? language = null,
        User? updatedBy = null)
    {
        return new DescriptionUpdate
        {
            Content = content,
            Language = language,
            UpdatedBy = updatedBy,
        };
    }

    public static DescriptionUpdate FromDescription(Description description, LocalizationHelper.Language? language,  User? updatedBy)
        => New(description.GetContent(language), language, updatedBy);
}