using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<User?> GetActiveUserAsync(CancellationToken ct)
    {
        //todo right now we get admin user
        return User.Admin();
    }
}