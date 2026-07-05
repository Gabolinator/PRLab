using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Policies;

public static class DataAccessPolicy
{
    public static void ValidateOwnership(
        DataOrigin origin,
        UserId? ownerUserId)
    {
        if (origin == DataOrigin.BuiltIn && ownerUserId.HasValue)
        {
            throw new InvalidOperationException(
                "Built-in data should not have an owner user id.");
        }

        if (origin != DataOrigin.BuiltIn && !ownerUserId.HasValue)
        {
            throw new InvalidOperationException(
                $"Data with origin '{origin}' must have an owner user id.");
        }
    }

    public static void ValidateVisibility(
        DataOrigin origin,
        VisibilityScope? visibilityScope)
    {
        if (!visibilityScope.HasValue)
        {
            return;
        }

        if (!Enum.IsDefined(visibilityScope.Value))
        {
            throw new InvalidOperationException(
                $"Unsupported visibility scope '{visibilityScope}'.");
        }

        if (origin == DataOrigin.BuiltIn &&
            visibilityScope.Value != VisibilityScope.Public)
        {
            throw new InvalidOperationException(
                "Built-in data must be public.");
        }
    }
}