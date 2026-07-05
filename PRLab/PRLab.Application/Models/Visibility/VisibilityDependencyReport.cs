namespace PRLab.Application.Models.Visibility;

public sealed record VisibilityDependencyReport
{
    public IReadOnlyList<VisibilityDependencyItem> PrivateDependencies { get; init; } = [];

    public bool HasPrivateDependencies => PrivateDependencies.Count > 0;
}