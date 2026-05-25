using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;

/// <summary>
/// DTO used for PUT operations on equipment resources.
/// </summary>
public record EquipmentPutDTO
{
    public EquipmentId? Id { get; set; }

    public UpsertOutcome Outcome { get; set; } = UpsertOutcome.Failed;

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DescriptionPutDTO Description { get; set; } = default!;

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; set; } = DataAuthority.Bidirectional;

    public string? UpdatedBy { get; set; }
}
