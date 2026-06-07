using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Muscle;

public sealed record MuscleAntagonistsPutDTO
{
    [Required]
    public IReadOnlyList<MuscleId> AntagonistIds { get; init; } = [];
}