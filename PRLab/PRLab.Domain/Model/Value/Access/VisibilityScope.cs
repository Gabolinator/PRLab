using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Policies;

namespace PRLab.Domain.Model.Value.Access;

public sealed record VisibilityInfo
{
    public VisibilityScope Scope { get; private init; }

    private VisibilityInfo()
    {
        // EF Core
    }

    private VisibilityInfo(VisibilityScope scope)
    {
        Scope = scope;
    }

    public static VisibilityInfo Private()
    {
        return new VisibilityInfo(VisibilityScope.Private);
    }

    public static VisibilityInfo Public()
    {
        return new VisibilityInfo(VisibilityScope.Public);
    }

    public static VisibilityInfo Shared()
    {
        return new VisibilityInfo(VisibilityScope.Shared);
    }
    
    
    public static VisibilityInfo FromPreferenceOrDefault(
        OwnershipInfo ownership,
        VisibilityScope? scopePreference = null)
    {
      
        if (scopePreference is null)
        {
            return VisibilityPolicy.DefaultFromOwnership(ownership);
        }

        VisibilityPolicy.ValidateScopeForOwnership(ownership, scopePreference.Value);

        return new VisibilityInfo(scopePreference.Value);
    }

    public bool IsVisibleToUser(UserId userId)
    {
        // todo implement shared eventually
        return Scope == VisibilityScope.Public;
    }
}