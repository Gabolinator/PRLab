using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Update;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class MuscleSeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IMuscleSeedFactory seedFactory) : EntitySeederBase(db)
{
    public override int Order => SeedPolicy.GetSeedOrder(DomainEnum.EntityType.Muscle);

    public override string Name => "DevelopmentMuscleSeed";

    public override string Version => "1.0.0";

    public override User SeedUser => userService.GetAdminUser("Seed");

    protected override async Task SeedEntityAsync(CancellationToken ct)
    {
        var muscleSeedItems = seedFactory.CreateInitialData();

        foreach (var muscleSeedItem in muscleSeedItems)
        {
            await ApplyMuscleSeedItem(muscleSeedItem, ct);
        }
    }

    private async Task<Muscle?> ApplyMuscleSeedItem(
        SeedItem<Muscle> muscleSeedItem,
        CancellationToken ct)
    {
        if (muscleSeedItem.Action == SeedAction.Ignore)
        {
            return null;
        }

        var seedMuscle = muscleSeedItem.Entity;

        var existingMuscle = await db.Muscles
            .Include(muscle => muscle.Description)
                .ThenInclude(description => description.Translations)
            .Include(muscle => muscle.Antagonists)
            .FirstOrDefaultAsync(
                muscle => muscle.NameKey == seedMuscle.NameKey,
                ct);

        if (existingMuscle is null)
        {
            await db.Muscles.AddAsync(seedMuscle, ct);

            return seedMuscle;
        }

        if (muscleSeedItem.Action == SeedAction.CreateIfMissing)
        {
            return existingMuscle;
        }

        existingMuscle.Update(
            MuscleUpdate.FromMuscle(
                seedMuscle,
                null,
                SeedUser));

        return existingMuscle;
    }
}