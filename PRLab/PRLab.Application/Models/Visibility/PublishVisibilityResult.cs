namespace PRLab.Application.Models.Visibility;

public sealed record PublishVisibilityResult
{
    public bool Published { get; init; }

    public bool RequiresConfirmation { get; init; }

    public VisibilityDependencyReport? DependencyReport { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = [];

    public static PublishVisibilityResult Success()
    {
        return new PublishVisibilityResult
        {
            Published = true
        };
    }

    public static PublishVisibilityResult NeedsConfirmation(
        VisibilityDependencyReport dependencyReport)
    {
        return new PublishVisibilityResult
        {
            RequiresConfirmation = true,
            DependencyReport = dependencyReport
        };
    }

    public static PublishVisibilityResult Failed(
        IReadOnlyList<string> errors)
    {
        return new PublishVisibilityResult
        {
            Errors = errors
        };
    }
}