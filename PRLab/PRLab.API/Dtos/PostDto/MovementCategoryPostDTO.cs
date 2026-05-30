using System.ComponentModel.DataAnnotations;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PostDto;

/// <summary>
/// Payload used to create a movement category and its descriptor metadata.
/// </summary>
public record MovementCategoryPostDTO
{
    public MovementCategoryId? Id { get; init; } = MovementCategoryId.New();

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    public MovementCategoryId? ParentCategoryId { get; init; }

    [Required]
    public IReadOnlyList<DomainEnum.BaseMovementCategory> BaseCategories { get; init; } =
        Array.Empty<DomainEnum.BaseMovementCategory>();

    public DescriptionPostDTO? Descriptor { get; init; }
}

public static class MovementCategoryPostDTOExtensions
{
    public static string Print(this MovementCategoryPostDTO  entity)
    {
        if (entity is null)
        {
            return "MovementCategoryPostDTO <null>";
        }

        var baseCategories = entity.BaseCategories is { Count: > 0 }
            ? string.Join(", ", entity.BaseCategories)
            : "none";

        var descriptor = entity.Descriptor is null
            ? "null"
            : $"{{DescriptionContent: \"{entity.Descriptor.Content}\" }}";

        return
            $"MovementCategoryPostDTO {{ Id: {entity.Id}, Name: \"{entity.Name}\", ParentCategoryId: {(entity.ParentCategoryId.HasValue ? entity.ParentCategoryId.ToString() : "null")}, BaseCategories: [{baseCategories}], Descriptor: {descriptor}}}";
    }
}
