using System.ComponentModel.DataAnnotations;

namespace PRLab.API.DTO.User;

public sealed record CurrentUserPutDTO
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public required string Name { get; init; }
}