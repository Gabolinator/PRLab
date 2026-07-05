using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Models.Visibility;

public sealed record PublishVisibilityOptions
{
    public bool CascadeToDependencies { get; init; }

    public User? ChangedBy { get; init; }
}