using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Value;

public sealed record AuditInfo(
    DateTimeOffset CreatedAt,
    UserId CreatedBy,
    DateTimeOffset? UpdatedAt = null,
    UserId? UpdatedBy = null,
    bool IsDeleted = false,
    DateTimeOffset? DeletedAt = null,
    UserId? DeletedBy = null
)
{
    private static UserId ResolveUserId(User? user)
    {
        return user?.Id ?? User.SystemUser.Id;
    }

    public static AuditInfo New(User? createdBy = null)
    {
        return new AuditInfo(
            CreatedAt: CoreUtilities.Clock.UtcNow,
            CreatedBy: ResolveUserId(createdBy)
        );
    }

    public AuditInfo MarkUpdated(User? updatedBy = null)
    {
        return this with
        {
            UpdatedAt = CoreUtilities.Clock.UtcNow,
            UpdatedBy = ResolveUserId(updatedBy)
        };
    }

    public AuditInfo MarkDeleted(User? deletedBy = null)
    {
        var userId = ResolveUserId(deletedBy);
        var now = CoreUtilities.Clock.UtcNow;

        return this with
        {
            IsDeleted = true,
            DeletedAt = now,
            DeletedBy = userId,
            UpdatedAt = now,
            UpdatedBy = userId
        };
    }

    public AuditInfo MarkRestored(User? restoredBy)
    {
        var userId = ResolveUserId(restoredBy);
        var now = CoreUtilities.Clock.UtcNow;

        return this with
        {
            IsDeleted = false,
            DeletedAt = now,
            DeletedBy = userId,
            UpdatedAt = now,
            UpdatedBy = userId
        };
    }
}