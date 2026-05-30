using System.ComponentModel.DataAnnotations;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;


public record MovementCategoryPutDTO
{

    [StringLength(256, MinimumLength = 2)]
    public string? Name { get; set; }

    public DomainEnum.BaseMovementCategory? BaseMovementCategory { get; init; }
    
    public DescriptionPutDTO? Description { get; set; } = default!;
}
