using System.ComponentModel.DataAnnotations;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;


public record MovementCategoryPutDTO
{
    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public MovementCategoryId? ParentCategoryId { get; set; }

    [Required]
    [MinLength(1)]
    public IReadOnlyList<DomainEnum.BaseMovementCategory> BaseCategories { get; set; } =
        Array.Empty<DomainEnum.BaseMovementCategory>();

    [Required]
    public DescriptionPutDTO Description { get; set; } = default!;
}
