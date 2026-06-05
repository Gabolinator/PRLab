using Microsoft.Extensions.Logging;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory.Muscle;

public sealed class DevelopmentMuscleAntagonistSeedFactory(ILogger<DevelopmentMuscleAntagonistSeedFactory> logger) : IMuscleAntagonistSeedFactory
{
    public IReadOnlyList<SeedRelationItem<MuscleId>> CreateInitialData(
        MuscleSeedCatalog muscleCatalog)
    {
        ArgumentNullException.ThrowIfNull(muscleCatalog);

        return
        [
            CreatePair(muscleCatalog, "Chest", "Lats", logger),
            CreatePair(muscleCatalog, "Front Delts", "Rear Delts", logger),
            CreatePair(muscleCatalog, "Biceps", "Triceps", logger),
            CreatePair(muscleCatalog, "Abs", "Spinal Erectors", logger),
            CreatePair(muscleCatalog, "Quads", "Hamstrings", logger),
            CreatePair(muscleCatalog, "Glutes", "Hip Flexors", logger),
            CreatePair(muscleCatalog, "Calves", "Shins", logger),
        ];
    }

    private static SeedRelationItem<MuscleId> CreatePair(
        MuscleSeedCatalog muscleCatalog,
        string firstMuscleName,
        string secondMuscleName,
        ILogger logger)
    {
        var firstMuscle = muscleCatalog.GetRequiredByName(firstMuscleName, logger);
        var secondMuscle = muscleCatalog.GetRequiredByName(secondMuscleName, logger);

        return new SeedRelationItem<MuscleId>(
            firstMuscle.Id,
            secondMuscle.Id,
            SeedAction.CreateIfMissing);
    }
}