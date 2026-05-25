using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PostDto;

public record MovementPostDTO
{
    public MovementId? Id { get; init; } = MovementId.New();

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
    
    public DescriptionPostDTO? Descriptor { get; init; }
    
    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; init; } = DataAuthority.Bidirectional;

    public string? CreatedBy { get; init; }

    public MovementId? VariantOfMovementGuid { get; init; }
    
    public IReadOnlyList<MuscleId>? PrimaryMusclesIds { get; init; }
    
    public IReadOnlyList<MuscleId>? SecondaryMusclesIds { get; init; }
    
    public IReadOnlyList<EquipmentId>? EquipmentIds { get; init; }
    
};