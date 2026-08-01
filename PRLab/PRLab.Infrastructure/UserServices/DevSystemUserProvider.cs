using PRLab.Application.Interface.UserService;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.UserServices;

public class DevSystemUserProvider : ISystemUserProvider
{
    public User GetSystemAdminUser(string? name = null)
    {
        return User.Admin(name);
    }
}