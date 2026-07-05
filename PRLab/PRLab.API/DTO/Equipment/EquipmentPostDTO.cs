using System.ComponentModel.DataAnnotations;
using PRLab.API.DTO.Description;
using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.API.DTO.Equipment;

/// <summary>
/// Payload used to create a new equipment entity along with its descriptor.
/// </summary>
public record EquipmentPostDTO
{
    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
    
    public DescriptionPostDTO? Descriptor { get; set; } = null;
    
    public VisibilityScope? VisibilityScope { get; init; }
}
