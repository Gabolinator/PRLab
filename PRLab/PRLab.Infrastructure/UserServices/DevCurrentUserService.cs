using Microsoft.Extensions.Options;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Application.Interface.UserService;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.UserServices.Authentication;

namespace PRLab.Infrastructure.UserServices;

public sealed class DevCurrentUserService(
    IUserRepository userRepository,
    IOptions<DevAuthenticationOptions> options)
    : ICurrentUserService
{
    public bool IsAuthenticated => true;
    
    private UserId CurrentUserId => UserId.FromGuid(CurrentGuid);
    
    private Guid CurrentGuid => options.Value.UserId ?? PredefinedUsers.Development.Id;
    
    public UserId GetRequiredUserId()
    {
        return CurrentUserId;
    }
    
    public async Task<User?> GetCurrentUserAsync(
        CancellationToken ct = default)
    {
        return await userRepository.GetByIdAsync(
            CurrentUserId,
            ct);
    }

    public async Task<User> GetRequiredCurrentUserAsync(
        CancellationToken ct = default)
    {
        return await GetRequiredDevelopmentUserAsync(ct);
    }
    
    private async Task<User> GetRequiredDevelopmentUserAsync(
        CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(
            CurrentUserId,
            ct);

        return user ?? throw new InvalidOperationException(
            $"Development user '{CurrentUserId}' was not initialized.");
    }
    
}