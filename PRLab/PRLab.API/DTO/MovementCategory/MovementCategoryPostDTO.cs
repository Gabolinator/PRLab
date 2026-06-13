using System.ComponentModel.DataAnnotations;
using PRLab.API.DTO.Description;
using PRLab.Domain;
using PRLab.Domain.Value.Enum.Movement;

namespace PRLab.API.DTO.MovementCategory;


public record MovementCategoryPostDTO
{
    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
    
    [Required]
    public BaseMovementCategory BaseCategory { get; init; }
    
    public DescriptionPostDTO? Description { get; init; }
}

public static class MovementCategoryPostDTOExtensions
{
    public static string Print(this MovementCategoryPostDTO  entity)
    {
        if (entity is null)
        {
            return "MovementCategoryPostDTO <null>";
        }

      

        var descriptor = entity.Description  is null
            ? "null"
            : $"{{DescriptionContent: \"{entity.Description .Content}\" }}";

        return
            $"MovementCategoryPostDTO {{ Name: \"{entity.Name}\",  BaseCategory: [{entity.BaseCategory}], Descriptor: {descriptor}}}";
    }
}
