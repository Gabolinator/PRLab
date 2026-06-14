using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories;

public class UserRepository(PRLabPgDBContext db, IClock clock) : IUserRepository
{
    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByNameAsync(string name, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<User>> ListAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<User> CreateAsync(User user, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<User> UpdateAsync(User user, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(UserId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}