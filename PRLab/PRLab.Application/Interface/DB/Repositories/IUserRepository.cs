using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken ct);

    Task<User?> GetByNameAsync(string name, CancellationToken ct);

    Task<IReadOnlyCollection<User>> ListAsync(CancellationToken ct);

    Task<User> CreateAsync(User user, CancellationToken ct);

    Task<User> UpdateAsync(User user, CancellationToken ct);

    Task<bool> ExistsAsync(UserId id, CancellationToken ct);
}