using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Infrastructure.DB.Seeding.Config;

public sealed class SeedingOptions
{
    public string SeedFileDirectory { get; init; } = string.Empty;

    public SeedingSource Source { get; init; } = SeedingSource.JsonFiles;

    public bool SeedingEnabled => Source != SeedingSource.None;

    public EntityType[]? EntitiesToSeed { get; init; }

    public EntityType[] EntitiesToOmit { get; init; } = [];

    public static EntityType[] DefaultEntitiesToSeed =>
    [
        EntityType.Equipment,
        EntityType.Muscle,
        EntityType.MovementCategory,
        EntityType.MuscleAntagonist,
        EntityType.Movement,
        EntityType.Exercise,
        EntityType.Workout
    ];

    public IReadOnlyCollection<EntityType> ResolveEntitiesToSeed()
    {
        if (EntitiesToSeed is { Length: > 0 } && EntitiesToOmit.Length > 0)
        {
            throw new InvalidOperationException(
                "Use either EntitiesToSeed or EntitiesToOmit, not both.");
        }

        if (EntitiesToSeed is { Length: > 0 })
        {
            return EntitiesToSeed.ToHashSet();
        }

        return DefaultEntitiesToSeed
            .Where(entityType => !EntitiesToOmit.Contains(entityType))
            .ToHashSet();
    }
}