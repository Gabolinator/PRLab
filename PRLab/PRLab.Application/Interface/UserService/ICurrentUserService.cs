using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Interface.UserService;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    UserId GetRequiredUserId();
    
    Task<User?> GetCurrentUserAsync(
        CancellationToken ct = default);

    Task<User> GetRequiredCurrentUserAsync(
        CancellationToken ct = default);
}