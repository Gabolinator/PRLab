using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;

public sealed record MuscleAntagonistsPutDTO
{
    [Required]
    public IReadOnlyList<MuscleId> AntagonistIds { get; init; } = [];
}