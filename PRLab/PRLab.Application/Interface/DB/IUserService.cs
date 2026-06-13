using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB;

public interface IUserService
{
    User GetSystemAdminUser(string? name = null);

    Task<User?> GetActiveUserAsync(CancellationToken ct);
}