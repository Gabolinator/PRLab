using System.ComponentModel.DataAnnotations;
using PRLab.API.DTO.Description;
using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.Anatomy;

namespace PRLab.API.DTO.Muscle;


public record MusclePutDTO
{
    [StringLength(256, MinimumLength = 2)]
    public string? Name { get; set; } = string.Empty;

    [StringLength(256)]
    public string? LatinName { get; set; }
    
    [EnumDataType(typeof(BodySection))]
    public BodySection? BodySection { get; set; } = null;
    
    public DescriptionPutDTO? Description { get; set; } = null;
}
