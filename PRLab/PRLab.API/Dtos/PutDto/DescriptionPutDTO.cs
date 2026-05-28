using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;

public record DescriptionPutDTO
{
    public DescriptionId? Id { get; set; } =null;

    public UpsertOutcome Outcome { get; set; } = UpsertOutcome.Failed;
    
    [Required]
    [StringLength(1024, MinimumLength = 3)]
    public string Content { get; set; } = string.Empty;

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; set; } = DataAuthority.Bidirectional;

    public string? UpdatedBy { get; set; }
}
