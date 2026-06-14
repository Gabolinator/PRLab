using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Muscle.Relation;

public sealed record MuscleAntagonistsPutDTO
{
    [Required]
    public IReadOnlyList<MuscleId> AntagonistIds { get; init; } = [];
}