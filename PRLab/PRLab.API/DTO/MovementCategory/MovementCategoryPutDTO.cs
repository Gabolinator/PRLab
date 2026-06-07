using System.ComponentModel.DataAnnotations;
using PRLab.API.DTO.Description;
using PRLab.Domain;

namespace PRLab.API.DTO.MovementCategory;


public record MovementCategoryPutDTO
{

    [StringLength(256, MinimumLength = 2)]
    public string? Name { get; set; }

    public DomainEnum.BaseMovementCategory? BaseMovementCategory { get; init; }
    
    public DescriptionPutDTO? Description { get; set; } = default!;
}
