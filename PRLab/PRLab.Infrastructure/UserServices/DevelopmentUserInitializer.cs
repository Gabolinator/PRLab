using Microsoft.Extensions.Options;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.UserServices.Authentication;

namespace PRLab.Infrastructure.UserServices;

public sealed class DevelopmentUserInitializer(
    IUserRepository userRepository,
    IOptions<DevAuthenticationOptions> options)
{
    public async Task EnsureCreatedAsync(
        CancellationToken ct = default)
    {
        var userId = UserId.FromGuid(
            options.Value.UserId ?? PredefinedUsers.Development.Id);

        if (await userRepository.ExistsAsync(userId, ct))
        {
            return;
        }

        var user = PredefinedUsers.Development.Create(
            options.Value.UserId,
            options.Value.UserName);

        await userRepository.CreateAsync(user, ct);
    }
}