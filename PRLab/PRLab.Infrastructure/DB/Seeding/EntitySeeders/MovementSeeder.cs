using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Update;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public class MovementSeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IMovementSeedFactory seedFactory) : EntitySeederBase(db)
{
    public override int Order => SeedPolicy.GetSeedOrder(DomainEnum.EntityType.Movement);

    public override string Name => "DevelopmentMovementSeed";

    public override string Version => "1.0.0";

    public override User SeedUser => userService.GetAdminUser("Seed");

    protected override async Task SeedEntityAsync(CancellationToken ct)
    {
        
        var equipmentCatalog = await SeedCatalogBuilder.CreateEquipmentCatalog(db, ct);
        var muscleCatalog = await SeedCatalogBuilder.CreateMuscleCatalog(db, ct);
        var movementCategoryCatalog = await SeedCatalogBuilder.CreateMovementCategoryCatalog(db, ct);
        
        var movementCategorySeedItems = seedFactory.CreateInitialData(
            equipmentCatalog,
            muscleCatalog,
            movementCategoryCatalog);

        foreach (var movementCategorySeedItem in movementCategorySeedItems)
        {
            await ApplyMovementSeedItem(movementCategorySeedItem, ct);
        }
    }

    private async Task<Movement?> ApplyMovementSeedItem(
        SeedItem<Movement> movementCategorySeedItem,
        CancellationToken ct)
    {
        if (movementCategorySeedItem.Action == SeedAction.Ignore)
        {
            return null;
        }

        var seedMovement = movementCategorySeedItem.Entity;

        var existingMovement = await db.Movements
            .Include(movementCategory => movementCategory.Description)
            .ThenInclude(description => description.Translations)
            .FirstOrDefaultAsync(
                movementCategory => movementCategory.NameKey == seedMovement.NameKey,
                ct);

        if (existingMovement is null)
        {
            await db.Movements.AddAsync(seedMovement, ct);

            return seedMovement;
        }

        if (movementCategorySeedItem.Action == SeedAction.CreateIfMissing)
        {
            return existingMovement;
        }

        existingMovement.Update(
            MovementUpdate.FromMovement(
                seedMovement,
                null,
                SeedUser));

        return existingMovement;
    }
}