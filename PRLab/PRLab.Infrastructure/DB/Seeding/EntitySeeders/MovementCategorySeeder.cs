using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Update;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class MovementCategorySeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IMovementCategorySeedFactory seedFactory) : EntitySeederBase(db)
{
    public override int Order => SeedPolicy.GetSeedOrder(DomainEnum.EntityType.MovementCategory);

    public override string Name => "DevelopmentMovementCategorySeed";

    public override string Version => "1.0.0";

    public override User SeedUser => userService.GetAdminUser("Seed");

    protected override async Task SeedEntityAsync(CancellationToken ct)
    {
        var movementCategorySeedItems = seedFactory.CreateInitialData();

        foreach (var movementCategorySeedItem in movementCategorySeedItems)
        {
            await ApplyMovementCategorySeedItem(movementCategorySeedItem, ct);
        }
    }

    private async Task<MovementCategory?> ApplyMovementCategorySeedItem(
        SeedItem<MovementCategory> movementCategorySeedItem,
        CancellationToken ct)
    {
        if (movementCategorySeedItem.Action == SeedAction.Ignore)
        {
            return null;
        }

        var seedMovementCategory = movementCategorySeedItem.Entity;

        var existingMovementCategory = await db.MovementCategories
            .Include(movementCategory => movementCategory.Description)
                .ThenInclude(description => description.Translations)
            .FirstOrDefaultAsync(
                movementCategory => movementCategory.NameKey == seedMovementCategory.NameKey,
                ct);

        if (existingMovementCategory is null)
        {
            await db.MovementCategories.AddAsync(seedMovementCategory, ct);

            return seedMovementCategory;
        }

        if (movementCategorySeedItem.Action == SeedAction.CreateIfMissing)
        {
            return existingMovementCategory;
        }

        existingMovementCategory.Update(
            MovementCategoryUpdate.FromMovementCategory(
                seedMovementCategory,
                null,
                SeedUser));

        return existingMovementCategory;
    }
}