using PRLab.Value.Identifier;

namespace PRLab.Model.Entity;

public sealed record Description
{
    public DescriptorId Id { get; init; }
    public string? Content { get; init; }

    private Description()
    {
        // EF Core
    }

    private Description(DescriptorId id, string? content)
    {
        Id = id;
        Content = Normalize(content);
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Content);

    public static Description New(string? content)
    {
        return new Description(DescriptorId.New(), content);
    }

    public static Description Existing(DescriptorId id, string? content)
    {
        return new Description(id, content);
    }

    public Description ChangeContent(string? content)
    {
        return this with
        {
            Content = Normalize(content)
        };
    }

    public Description RemoveContent()
    {
        return ChangeContent(null);
    }

    private static string? Normalize(string? content)
    {
        return string.IsNullOrWhiteSpace(content)
            ? null
            : content.Trim();
    }
}