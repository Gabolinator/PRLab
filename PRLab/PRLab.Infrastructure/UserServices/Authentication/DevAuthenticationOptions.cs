namespace PRLab.Infrastructure.UserServices.Authentication;

public sealed class DevAuthenticationOptions
{
    public required Guid? UserId { get; init; }
    public string? UserName { get; set; }
}