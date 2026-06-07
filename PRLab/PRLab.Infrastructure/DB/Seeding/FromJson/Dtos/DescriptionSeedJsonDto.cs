using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

public sealed record DescriptionSeedJsonDto
{
    public string? Content { get; init; }

    public static DescriptionSeedJsonDto FromDescription(Description description)
    {
        ArgumentNullException.ThrowIfNull(description);

        return new DescriptionSeedJsonDto
        {
            Content = description.GetContent(),
        };
    }

    public Description ToDescription()
    {
        return string.IsNullOrWhiteSpace(Content)
            ? Description.None()
            : Description.New(Content);
    }
}