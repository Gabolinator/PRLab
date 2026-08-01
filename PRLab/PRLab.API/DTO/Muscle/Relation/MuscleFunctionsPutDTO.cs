using System.ComponentModel.DataAnnotations;

namespace PRLab.API.DTO.Muscle.Relation;

// whole replacement
public sealed record MuscleFunctionsPutDTO
{
    [Required]
    public IReadOnlyList<MuscleFunctionInputDTO> Functions { get; init; } = [];
}