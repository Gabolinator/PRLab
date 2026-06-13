using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Muscle;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Update;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class MuscleSeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IMuscleSeedFactory seedFactory,
    IAppLogger logger) : EntitySeederBase(db, logger)
{
    public override string Name => "DevelopmentMuscleSeed";

    public override string Version => "1.0.0";

    public override DomainEnum.EntityType EntityType => DomainEnum.EntityType.Muscle;

    public override User SeedUser => userService.GetSystemAdminUser("Seed");
    
    protected override async Task<IReadOnlyList<SeedChange>> SeedEntityAsync(CancellationToken ct)
    {
        var muscleSeedItems = seedFactory.CreateInitialData();

        var changes = new List<SeedChange>();

        foreach (var muscleSeedItem in muscleSeedItems)
        {
            var result = await ApplyMuscleSeedItem(muscleSeedItem, ct);

            if (result.change is not null)
            {
                changes.Add(result.change);
            }
        }

        return changes;
    }

    private async Task<(Muscle? entity, SeedChange? change)> ApplyMuscleSeedItem(
        SeedItem<Muscle> muscleSeedItem,
        CancellationToken ct)
    {
        if (muscleSeedItem.Action == SeedAction.Ignore)
        {
            return (null, null);
        }

        var seedMuscle = muscleSeedItem.Entity;

        var existingMuscle = await db.Muscles
            .Include(muscle => muscle.Description)
                .ThenInclude(description => description.Translations)
            .FirstOrDefaultAsync(
                muscle => muscle.NameKey == seedMuscle.NameKey,
                ct);

        if (existingMuscle is null)
        {
            await db.Muscles.AddAsync(seedMuscle, ct);

            logger.Log($"Seeded - {EntityType} : {seedMuscle.NameKey}");

            return (
                seedMuscle,
                new SeedChange(
                    seedMuscle.NameKey,
                    SeedChangeType.Created));
        }

        if (muscleSeedItem.Action == SeedAction.CreateIfMissing)
        {
            return (existingMuscle, null);
        }

        logger.Log($"Seeder Updating - {EntityType} : {seedMuscle.NameKey}");

        var hasChanged = existingMuscle.Update(
            MuscleUpdate.FromMuscle(
                seedMuscle,
                null,
                SeedUser));

        return hasChanged
            ? (
                existingMuscle,
                new SeedChange(
                    seedMuscle.NameKey,
                    SeedChangeType.Updated))
            : (existingMuscle, null);
    }
}