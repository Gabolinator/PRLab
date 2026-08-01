using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.API.DTO.User;

public sealed record UserPostDTO
{
    [Required] [StringLength(150, MinimumLength = 2)] public required string Name { get; init; }
    [EnumDataType(typeof(UserRole))] 
    public UserRole Role { get; init; } = UserRole.User;
}