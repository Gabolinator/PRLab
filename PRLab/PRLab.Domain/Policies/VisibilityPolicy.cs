using PRLab.Domain.Model.Value.Access;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Ownership;

namespace PRLab.Domain.Policies;

public static class VisibilityPolicy
{
    public static VisibilityInfo DefaultFromOwnership(OwnershipInfo ownership)
    {
        ArgumentNullException.ThrowIfNull(ownership);

        return ownership.Origin switch
        {
            DataOrigin.BuiltIn => VisibilityInfo.Public(),
            DataOrigin.UserCreated => VisibilityInfo.Private(),
            DataOrigin.CoachCreated => VisibilityInfo.Shared(),
            DataOrigin.Imported => VisibilityInfo.Private(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(ownership),
                ownership.Origin,
                null)
        };
    }

    public static void ValidateVisibilityForOwnership(
        OwnershipInfo ownership,
        VisibilityInfo visibility)
    {
        var scope = visibility.Scope;
        
       ValidateScopeForOwnership(ownership, scope);
    }

    public static void ValidateScopeForOwnership(OwnershipInfo ownership, VisibilityScope scope)
    {
        var origin = ownership.Origin;
        
        if (origin == DataOrigin.BuiltIn && scope != VisibilityScope.Public)
        {
            throw new ArgumentException(
                "Built-in data must be public.",
                nameof(scope));
        }

        if (origin == DataOrigin.UserCreated && scope == VisibilityScope.Shared)
        {
            // todo
            // Optional rule.
            // You may allow this later once AccessGrant exists.
            return;
        }

        if (origin == DataOrigin.Imported && scope == VisibilityScope.Public)
        {
            // todo
            // Optional rule.
            // Imported objects probably should not become public by default.
            return;
        }
    }
}