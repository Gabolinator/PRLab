using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PostDto;

/// <summary>
/// Payload used to create a new equipment entity along with its descriptor.
/// </summary>
public record EquipmentPostDTO
{
    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
    
    public DescriptionPostDTO? Descriptor { get; set; } = null;
}
