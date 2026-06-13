using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB;

public class UserService(IUserRepository userRepository) : IUserService
{
    public User GetSystemAdminUser(string? name = null)
    {
        return User.Admin(name);
    }

    public async Task<User?> GetActiveUserAsync(CancellationToken ct)
    {
        // TODO: Later, resolve from auth claims / HttpContext.
        // For now, use a stable development user, not Admin.
        return await GetOrCreateDevelopmentUserAsync(ct);
    }

    private async Task<User> GetOrCreateDevelopmentUserAsync(CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(User.DevelopmentUser.Id, ct);

        if (user is not null)
        {
            return user;
        }

        var developmentUser = User.DevelopmentUser.Create();

        return await userRepository.CreateAsync(developmentUser, ct);
    }
}