using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Application.Models.Visibility;

public interface IVisibilityPublisher
{
    Task<PublishVisibilityResult> MakePublicAsync(
        EntityType entityType,
        Guid entityId,
        PublishVisibilityOptions options,
        CancellationToken ct = default);
}