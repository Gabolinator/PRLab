using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Update;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class MovementCategorySeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IMovementCategorySeedFactory seedFactory,
    IAppLogger logger) : EntitySeederBase(db, logger)
{
    public override string Name => "DevelopmentMovementCategorySeed";

    public override string Version => "1.0.1";

    public override DomainEnum.EntityType EntityType => DomainEnum.EntityType.MovementCategory;

    public override User SeedUser => userService.GetAdminUser("Seed");

    protected override async Task<IReadOnlyList<SeedChange>> SeedEntityAsync(CancellationToken ct)
    {
        var movementCategorySeedItems = seedFactory.CreateInitialData();

        var changes = new List<SeedChange>();

        foreach (var movementCategorySeedItem in movementCategorySeedItems)
        {
            var result = await ApplyMovementCategorySeedItem(movementCategorySeedItem, ct);

            if (result.change is not null)
            {
                changes.Add(result.change);
            }
        }

        return changes;
    }

    private async Task<(MovementCategory? entity, SeedChange? change)> ApplyMovementCategorySeedItem(
        SeedItem<MovementCategory> movementCategorySeedItem,
        CancellationToken ct)
    {
        if (movementCategorySeedItem.Action == SeedAction.Ignore)
        {
            return (null, null);
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

            logger.Log($"Seeded - {EntityType} : {seedMovementCategory.NameKey}");

            return (
                seedMovementCategory,
                new SeedChange(
                    seedMovementCategory.NameKey,
                    SeedChangeType.Created));
        }

        if (movementCategorySeedItem.Action == SeedAction.CreateIfMissing)
        {
            return (existingMovementCategory, null);
        }

        logger.Log($"Seeder Updating - {EntityType} : {seedMovementCategory.NameKey}");

        var hasChanged = existingMovementCategory.Update(
            MovementCategoryUpdate.FromMovementCategory(
                seedMovementCategory,
                null,
                SeedUser));

        return hasChanged
            ? (
                existingMovementCategory,
                new SeedChange(
                    seedMovementCategory.NameKey,
                    SeedChangeType.Updated))
            : (existingMovementCategory, null);
    }
}