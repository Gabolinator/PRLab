using System.ComponentModel.DataAnnotations;
using PRLab.Domain;

namespace PRLab.API.Dtos.PutDto;


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
