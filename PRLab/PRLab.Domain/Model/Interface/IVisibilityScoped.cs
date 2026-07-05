using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Access;

namespace PRLab.Domain.Model.Interface;

public interface IVisibilityScoped
{
    VisibilityInfo Visibility { get; }

    bool UpdateVisibility(
        VisibilityInfo visibility,
        User? changedBy = null);

    bool MakePublic(User? changedBy = null)
    {
        return UpdateVisibility(
            VisibilityInfo.Public(),
            changedBy);
    }

    bool MakePrivate(User? changedBy = null)
    {
        return UpdateVisibility(
            VisibilityInfo.Private(),
            changedBy);
    }

    bool MakeShared(User? changedBy = null)
    {
        return UpdateVisibility(
            VisibilityInfo.Shared(),
            changedBy);
    }
}