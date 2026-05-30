using System.ComponentModel.DataAnnotations;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PostDto;

/// <summary>
/// Payload used to create a new muscle along with descriptor metadata.
/// </summary>
public record MusclePostDTO
{
    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(256)]
    public string? LatinName { get; init; }

    [Required]
    [EnumDataType(typeof(DomainEnum.BodySection))]
    public DomainEnum.BodySection BodySection { get; init; } = default;

    public DescriptionPostDTO? Descriptor { get; init; }

    public IReadOnlyList<MuscleId>? AntagonistIds { get; init; }
}

public static class MusclePostDTOExtensions
{
    public static string Print(this MusclePostDTO entity)
    {
        if (entity is null)
        {
            return "MusclePostDTO <null>";
        }

        var antagonists = entity.AntagonistIds is { Count: > 0 }
            ? string.Join(", ", entity.AntagonistIds)
            : "none";

        var descriptor = entity.Descriptor is null
            ? "null"
            : $"{{DescriptionContent: \"{entity.Descriptor.Content}\"}}";

        return
            $"MusclePostDTO {{ Name: \"{entity.Name}\", BodySection  {entity.BodySection} Antagonists: [{antagonists}], Descriptor: {descriptor}}}";

    }
}

