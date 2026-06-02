using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Catalog;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;

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
        var muscleCatalog = await SeedCatalogBuilder.CreateMuscleCatalog(db, ct);

        var muscleAntagonistSeedItems = seedFactory.CreateInitialData(muscleCatalog);
        
        foreach (var muscleAntagonistSeedItem in muscleAntagonistSeedItems)
        {
            ApplyMuscleAntagonistSeedItem(
                muscleAntagonistSeedItem,
                muscleCatalog);
        }
    }

    private static void ApplyMuscleAntagonistSeedItem(
        SeedRelationItem<MuscleId> muscleAntagonistSeedItem,
        MuscleSeedCatalog muscleCatalog)
    {
        if (muscleAntagonistSeedItem.Action == SeedAction.Ignore)
        {
            return;
        }

        var sourceMuscle = muscleCatalog.GetRequiredById(
            muscleAntagonistSeedItem.SourceId);

        var targetMuscle = muscleCatalog.GetRequiredById(
            muscleAntagonistSeedItem.TargetId);

        AddAntagonistPairIfMissing(sourceMuscle, targetMuscle);
    }

    private static void AddAntagonistPairIfMissing(
        Muscle sourceMuscle,
        Muscle targetMuscle)
    {
        sourceMuscle.AddAntagonist(targetMuscle.Id);
        targetMuscle.AddAntagonist(sourceMuscle.Id);
    }
}