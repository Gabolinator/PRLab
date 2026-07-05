using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Application.Models.Visibility;

public sealed record VisibilityDependencyItem
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required EntityType EntityType { get; init; }

    public required VisibilityScope CurrentVisibility { get; init; }
}