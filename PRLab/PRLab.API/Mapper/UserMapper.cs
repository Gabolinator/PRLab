using PRLab.API.DTO.User;
using PRLab.Domain.Model.Entity;

namespace PRLab.API.Mapper;

public static class UserMapper
{
    public static UserGetDTO ToGetDTO(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserGetDTO(
            user.Id,
            user.Name,
            user.Role,
            user.Audit.CreatedAt);
    }

    public static IReadOnlyList<UserGetDTO> ToGetDTOs(
        IReadOnlyCollection<User> users)
    {
        ArgumentNullException.ThrowIfNull(users);

        return users
            .Select(ToGetDTO)
            .ToList();
    }

    public static UserSummaryDTO ToSummaryDTO(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserSummaryDTO(
            user.Id,
            user.Name);
    }

    public static User ToEntity(UserPostDTO payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return User.New(
            payload.Name,
            payload.Role);
    }
}