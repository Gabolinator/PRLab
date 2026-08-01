using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.User;

public sealed record UserSummaryDTO(
    UserId Id,
    string Name);