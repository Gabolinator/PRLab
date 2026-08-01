using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.User;

public sealed record UserGetDTO(
    UserId Id,
    string Name,
    UserRole Role,
    DateTimeOffset CreatedAtUtc);