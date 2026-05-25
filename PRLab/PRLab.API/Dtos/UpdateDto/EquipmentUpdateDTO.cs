using System.ComponentModel.DataAnnotations;

namespace PRLab.API.Dtos.UpdateDto;

/// <summary>
/// DTO used for PATCH requests targeting equipment resources.
/// </summary>
public record EquipmentUpdateDTO
{
    [StringLength(256, MinimumLength = 2)]
    public string? Name { get; init; }
    public DescriptorUpdateDTO? Descriptor { get; set; } = null;
    
    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; init; }

    public string? UpdatedBy { get; init; }
}
