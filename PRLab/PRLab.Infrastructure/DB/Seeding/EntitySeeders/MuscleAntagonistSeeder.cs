using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Enum.System;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class MuscleAntagonistSeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IMuscleAntagonistSeedFactory seedFactory,
    IAppLogger logger) : EntitySeederBase(db, logger)
{
    public override string Name => "DevelopmentMuscleAntagonistSeed";

    public override string Version => "1.0.0";

    public override EntityType EntityType => EntityType.MuscleAntagonist;

    public override User SeedUser => userService.GetSystemAdminUser("Seed");

    protected override async Task<IReadOnlyList<SeedChange>> SeedEntityAsync(CancellationToken ct)
    {
        var muscleCatalog = await SeedCatalogBuilder.CreateMuscleCatalog(db, ct);

        var muscleAntagonistSeedItems = seedFactory.CreateInitialData(muscleCatalog);

        var changes = new List<SeedChange>();

        foreach (var muscleAntagonistSeedItem in muscleAntagonistSeedItems)
        {
            var change = ApplyMuscleAntagonistSeedItem(
                muscleAntagonistSeedItem,
                muscleCatalog);

            if (change is not null)
            {
                changes.Add(change);
            }
        }

        return changes;
    }

    private static SeedChange? ApplyMuscleAntagonistSeedItem(
        SeedRelationItem<MuscleId> muscleAntagonistSeedItem,
        MuscleSeedCatalog muscleCatalog)
    {
        if (muscleAntagonistSeedItem.Action == SeedAction.Ignore)
        {
            return null;
        }

        var sourceMuscle = muscleCatalog.GetRequiredById(
            muscleAntagonistSeedItem.SourceId);

        var targetMuscle = muscleCatalog.GetRequiredById(
            muscleAntagonistSeedItem.TargetId);

        var changed = AddAntagonistPairIfMissing(
            sourceMuscle,
            targetMuscle);

        if (!changed)
        {
            return null;
        }

        return new SeedChange(
            $"{sourceMuscle.NameKey}->{targetMuscle.NameKey}",
            SeedChangeType.Created);
    }

    private static bool AddAntagonistPairIfMissing(
        Muscle sourceMuscle,
        Muscle targetMuscle)
    {
        var sourceAlreadyHasTarget = sourceMuscle.Antagonists
            .Any(antagonist => antagonist.AntagonistMuscleId == targetMuscle.Id);

        var targetAlreadyHasSource = targetMuscle.Antagonists
            .Any(antagonist => antagonist.AntagonistMuscleId == sourceMuscle.Id);

        if (sourceAlreadyHasTarget && targetAlreadyHasSource)
        {
            return false;
        }

        sourceMuscle.AddAntagonist(targetMuscle.Id);
        targetMuscle.AddAntagonist(sourceMuscle.Id);

        return true;
    }
}