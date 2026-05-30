using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB;

public interface IUserService
{
    Task<User?> GetActiveUserAsync(CancellationToken ct);
}