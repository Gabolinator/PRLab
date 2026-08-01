using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Repositories;

public sealed class UserRepository(
    PRLabPgDBContext db)
    : IUserRepository
{
    public async Task<User?> GetByIdAsync(
        UserId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await db.Users
            .ForFullRead()
            .FirstOrDefaultAsync(
                user => user.Id == id,
                ct);
    }

    public async Task<User?> GetByNameAsync(
        string name,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "User name cannot be empty.",
                nameof(name));
        }

        var normalizedName = name.Trim();

        return await db.Users
            .ForFullRead()
            .FirstOrDefaultAsync(
                user => user.Name == normalizedName,
                ct);
    }

    public async Task<IReadOnlyCollection<User>> ListAsync(
        CancellationToken ct)
    {
        return await db.Users
            .ForFullRead()
            .OrderBy(user => user.Name)
            .ToListAsync(ct);
    }

    public async Task<User> CreateAsync(
        User user,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(user);

        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);

        return user;
    }

    public async Task<User> UpdateAsync(
        User user,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(user);
        ValidateId(user.Id);

        await db.SaveChangesAsync(ct);

        return user;
    }

    public async Task<User?> GetForUpdateAsync(
        UserId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await db.Users
            .ForFullWrite()
            .FirstOrDefaultAsync(
                user => user.Id == id,
                ct);
    }
    
    public async Task<bool> ExistsAsync(
        UserId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await db.Users
            .ActiveOnly()
            .AsNoTracking()
            .AnyAsync(
                user => user.Id == id,
                ct);
    }

    private static void ValidateId(UserId id) => DomainGuard.ValidRequiredId(id, nameof(id));
   
}