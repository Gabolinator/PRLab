using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Catalog;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.Seeding.Factory.Muscle;

public sealed class DevelopmentMuscleAntagonistSeedFactory : IMuscleAntagonistSeedFactory
{
    public IReadOnlyList<SeedRelationItem<MuscleId>> CreateInitialData(
        MuscleSeedCatalog muscleCatalog)
    {
        ArgumentNullException.ThrowIfNull(muscleCatalog);

        return
        [
            CreatePair(muscleCatalog, "Chest", "Lats"),
            CreatePair(muscleCatalog, "Front Delts", "Rear Delts"),
            CreatePair(muscleCatalog, "Biceps", "Triceps"),
            CreatePair(muscleCatalog, "Abs", "Spinal Erectors"),
            CreatePair(muscleCatalog, "Quads", "Hamstrings"),
            CreatePair(muscleCatalog, "Glutes", "Hip Flexors"),
            CreatePair(muscleCatalog, "Calves", "Shins"),
        ];
    }

    private static SeedRelationItem<MuscleId> CreatePair(
        MuscleSeedCatalog muscleCatalog,
        string firstMuscleName,
        string secondMuscleName)
    {
        var firstMuscle = muscleCatalog.GetRequiredByName(firstMuscleName);
        var secondMuscle = muscleCatalog.GetRequiredByName(secondMuscleName);

        return new SeedRelationItem<MuscleId>(
            firstMuscle.Id,
            secondMuscle.Id,
            SeedAction.CreateIfMissing);
    }
}