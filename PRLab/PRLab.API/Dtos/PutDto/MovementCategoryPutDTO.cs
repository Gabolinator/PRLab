using System.ComponentModel.DataAnnotations;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;

/// <summary>
/// DTO used for PUT operations on movement category resources.
/// </summary>
public record MovementCategoryPutDTO
{
    public MovementCategoryId? Id { get; set; }

    public UpsertOutcome Outcome { get; set; } = UpsertOutcome.Failed;

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

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; set; } = DataAuthority.Bidirectional;

    public string? UpdatedBy { get; set; }
}
