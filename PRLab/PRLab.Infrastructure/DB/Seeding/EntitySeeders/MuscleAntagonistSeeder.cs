using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class MuscleAntagonistSeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IMuscleAntagonistSeedFactory seedFactory) : EntitySeederBase(db)
{
    public override int Order => SeedPolicy.GetSeedOrder(DomainEnum.EntityType.MuscleAntagonist);

    public override string Name => "DevelopmentMuscleAntagonistSeed";

    public override string Version => "1.0.0";

    public override User SeedUser => userService.GetAdminUser("Seed");

    protected override async Task SeedEntityAsync(CancellationToken ct)
    {
        var muscleAntagonistSeedItems = seedFactory.CreateInitialData();

        var musclesByKey = await db.Muscles
            .Include(muscle => muscle.Antagonists)
            .ToDictionaryAsync(
                muscle => SeedKeyGenerator.GenerateMuscleKey(muscle),
                muscle => muscle,
                ct);

        foreach (var muscleAntagonistSeedItem in muscleAntagonistSeedItems)
        {
            ApplyMuscleAntagonistSeedItem(
                muscleAntagonistSeedItem,
                musclesByKey);
        }
    }

    private static void ApplyMuscleAntagonistSeedItem(
        SeedRelationItem muscleAntagonistSeedItem,
        IReadOnlyDictionary<string, Muscle> musclesByKey)
    {
        if (muscleAntagonistSeedItem.Action == SeedAction.Ignore)
        {
            return;
        }

        var sourceMuscle = GetRequiredMuscle(
            musclesByKey,
            muscleAntagonistSeedItem.SourceKey);

        var targetMuscle = GetRequiredMuscle(
            musclesByKey,
            muscleAntagonistSeedItem.TargetKey);

        AddAntagonistPairIfMissing(sourceMuscle, targetMuscle);
    }

    private static Muscle GetRequiredMuscle(
        IReadOnlyDictionary<string, Muscle> musclesByKey,
        string muscleKey)
    {
        if (musclesByKey.TryGetValue(muscleKey, out var muscle))
        {
            return muscle;
        }

        throw new InvalidOperationException(
            $"Seed muscle '{muscleKey}' was not found.");
    }

    private static void AddAntagonistPairIfMissing(
        Muscle sourceMuscle,
        Muscle targetMuscle)
    {
        sourceMuscle.AddAntagonist(targetMuscle.Id);
        targetMuscle.AddAntagonist(sourceMuscle.Id);
    }
}