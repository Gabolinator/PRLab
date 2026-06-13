using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Value.Ownership;

public sealed record OwnershipInfo
{
    public DomainEnum.DataOrigin Origin { get; private init; }

    public UserId? OwnerUserId { get; private init; }

    private OwnershipInfo()
    {
        // EF Core
    }

    private OwnershipInfo(
        DomainEnum.DataOrigin origin,
        UserId? ownerUserId)
    {
        Origin = origin;
        OwnerUserId = ownerUserId;
    }

    public static OwnershipInfo BuiltIn()
    {
        return new OwnershipInfo(
            DomainEnum.DataOrigin.BuiltIn,
            null
        );
    }

    public static OwnershipInfo UserCreated(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new OwnershipInfo(
            DomainEnum.DataOrigin.UserCreated,
            user.Id
        );
    }

    public static OwnershipInfo CoachCreated(User coach)
    {
        ArgumentNullException.ThrowIfNull(coach);

        if (coach.Role != DomainEnum.UserRole.Coach &&
            coach.Role != DomainEnum.UserRole.Admin)
        {
            throw new InvalidOperationException("Only coaches or admins can create coach-owned data.");
        }

        return new OwnershipInfo(
            DomainEnum.DataOrigin.CoachCreated,
            coach.Id
        );
    }

    public static OwnershipInfo Imported(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new OwnershipInfo(
            DomainEnum.DataOrigin.Imported,
            user.Id
        );
    }

    public bool IsBuiltIn => Origin == DomainEnum.DataOrigin.BuiltIn;

    public bool IsOwnedBy(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return OwnerUserId == user.Id;
    }
}