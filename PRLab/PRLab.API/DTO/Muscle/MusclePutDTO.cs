using System.ComponentModel.DataAnnotations;
using PRLab.API.DTO.Description;
using PRLab.Domain;

namespace PRLab.API.DTO.Muscle;


public record MusclePutDTO
{
    [StringLength(256, MinimumLength = 2)]
    public string? Name { get; set; } = string.Empty;

    [StringLength(256)]
    public string? LatinName { get; set; }
    
    [EnumDataType(typeof(DomainEnum.BodySection))]
    public DomainEnum.BodySection? BodySection { get; set; } = null;
    
    public DescriptionPutDTO? Description { get; set; } = null;
}
