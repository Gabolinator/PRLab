using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;

/// <summary>
/// DTO used for PUT operations on equipment resources.
/// </summary>
public record EquipmentPutDTO
{
    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DescriptionPutDTO Description { get; set; } = default!;
}
