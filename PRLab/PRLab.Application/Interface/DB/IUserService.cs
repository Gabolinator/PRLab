using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB;

public interface IUserService
{
    User GetAdminUser(string? name = null);
    
    Task<User?> GetActiveUserAsync(CancellationToken ct);
}