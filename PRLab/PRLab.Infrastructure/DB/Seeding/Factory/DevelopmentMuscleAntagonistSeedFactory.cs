using PRLab.Application.Interface.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.Factory;

public sealed class DevelopmentMuscleAntagonistSeedFactory : IMuscleAntagonistSeedFactory
{
    public IReadOnlyList<SeedRelationItem> CreateInitialData()
    {
        return
        [
            CreatePair("Chest", "Lats"),
            CreatePair("Front Delts", "Rear Delts"),
            CreatePair("Biceps", "Triceps"),
            CreatePair("Abs", "Spinal Erectors"),
            CreatePair("Quads", "Hamstrings"),
            CreatePair("Glutes", "Hip Flexors"),
            CreatePair("Calves", "Shins"),
        ];
    }

    private static SeedRelationItem CreatePair(
        string firstMuscleName,
        string secondMuscleName)
    {
        return new SeedRelationItem(
            SeedKeyGenerator.GenerateMuscleKeyFromName(firstMuscleName),
            SeedKeyGenerator.GenerateMuscleKeyFromName(secondMuscleName),
            SeedAction.CreateIfMissing);
    }
}