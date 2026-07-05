using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Application.Models.Visibility;

public interface IVisibilityDependencyValidator
{
    public interface IVisibilityDependencyValidator
    {
        Task<VisibilityDependencyReport> GetPublicDependencyReportAsync(
            EntityType entityType,
            Guid entityId,
            CancellationToken ct = default);
    }
}