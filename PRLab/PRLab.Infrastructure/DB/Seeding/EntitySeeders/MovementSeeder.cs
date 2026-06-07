using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Update;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public class MovementSeeder(
    PRLabPgDBContext db,
    IUserService userService,
    IMovementSeedFactory seedFactory,
    IAppLogger logger) : EntitySeederBase(db, logger)
{
    public override string Name => "DevelopmentMovementSeed";

    public override string Version => "1.0.2";

    public override DomainEnum.EntityType EntityType => DomainEnum.EntityType.Movement;

    public override User SeedUser => userService.GetAdminUser("Seed");

    protected override async Task<IReadOnlyList<SeedChange>> SeedEntityAsync(CancellationToken ct)
    {
        var equipmentCatalog = await SeedCatalogBuilder.CreateEquipmentCatalog(db, ct);
        var muscleCatalog = await SeedCatalogBuilder.CreateMuscleCatalog(db, ct);
        var movementCategoryCatalog = await SeedCatalogBuilder.CreateMovementCategoryCatalog(db, ct);
        var movementCatalog = await SeedCatalogBuilder.CreateMovementCatalog(db, ct);
        
        var movementSeedItems = seedFactory.CreateInitialData(
           new MovementSeedCatalogs(
               equipmentCatalog,
               muscleCatalog,
               movementCategoryCatalog,
               movementCatalog));

        var changes = new List<SeedChange>();

        foreach (var movementSeedItem in movementSeedItems)
        {
            var result = await ApplyMovementSeedItem(movementSeedItem, ct);

            if (result.change is not null)
            {
                changes.Add(result.change);
            }
        }

        return changes;
    }

    private async Task<(Movement? entity, SeedChange? change)> ApplyMovementSeedItem(
        SeedItem<Movement> movementSeedItem,
        CancellationToken ct)
    {
        if (movementSeedItem.Action == SeedAction.Ignore)
        {
            return (null, null);
        }

        var seedMovement = movementSeedItem.Entity;

        var existingMovement = await db.Movements
            .Include(movement => movement.Description)
                .ThenInclude(description => description.Translations)
            .Include(movement => movement.MovementCategory)
            .Include(movement => movement.Muscles)
            .Include(movement => movement.Equipments)
            .Include(movement => movement.Patterns)
            .FirstOrDefaultAsync(
                movement => movement.NameKey == seedMovement.NameKey,
                ct);

        if (existingMovement is null)
        {
            await db.Movements.AddAsync(seedMovement, ct);

            logger.Log($"Seeded - {EntityType} : {seedMovement.NameKey}");

            return (
                seedMovement,
                new SeedChange(
                    seedMovement.NameKey,
                    SeedChangeType.Created));
        }

        if (movementSeedItem.Action == SeedAction.CreateIfMissing)
        {
            return (existingMovement, null);
        }

        logger.Log($"Seeder Updating - {EntityType} : {seedMovement.NameKey}");

        var hasChanged = existingMovement.Update(
            MovementUpdate.FromMovement(
                seedMovement,
                null,
                SeedUser));

        return hasChanged
            ? (
                existingMovement,
                new SeedChange(
                    seedMovement.NameKey,
                    SeedChangeType.Updated))
            : (existingMovement, null);
    }
}