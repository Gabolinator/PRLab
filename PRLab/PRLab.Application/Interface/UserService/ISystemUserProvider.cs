using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.UserService;

public interface ISystemUserProvider
{
    User GetSystemAdminUser(string? name = null);
}