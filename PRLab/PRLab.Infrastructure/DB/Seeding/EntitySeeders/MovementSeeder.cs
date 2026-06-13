using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Movement;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Enum.System;
using PRLab.Domain.Value.Identifier;
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

    public override string Version => "1.0.0";

    public override EntityType EntityType => EntityType.Movement;

    public override User SeedUser => userService.GetSystemAdminUser("Seed");

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
        
        await db.SaveChangesAsync(ct);

        movementCatalog = await SeedCatalogBuilder.CreateMovementCatalog(db, ct);

        if (seedFactory is IMovementVariantSeedFactory variantSeedFactory)
        {
            var variantRelations = variantSeedFactory.CreateVariantInitialData(
                movementCatalog);

            foreach (var variantRelation in variantRelations)
            {
                var change = await ApplyMovementVariantSeedItem(
                    variantRelation,
                    movementCatalog,
                    ct);

                if (change is not null)
                {
                    changes.Add(change);
                }
            }
        }

        return changes;
    }

    private async Task<SeedChange?> ApplyMovementVariantSeedItem(
    SeedRelationItem<MovementId> movementVariantSeedItem,
    MovementSeedCatalog movementCatalog,
    CancellationToken ct)
    {
        if (movementVariantSeedItem.Action == SeedAction.Ignore)
        {
            return null;
        }

        var sourceMovement = await db.Movements
            .FirstOrDefaultAsync(
                movement => movement.Id == movementVariantSeedItem.SourceId,
                ct);

        var parentMovement = movementCatalog.GetRequiredById(
            movementVariantSeedItem.TargetId);

        if (sourceMovement is null)
        {
            throw new InvalidOperationException(
                $"Movement '{movementVariantSeedItem.SourceId.Value}' was not found.");
        }
        
        if (sourceMovement.VariantOfId == parentMovement.Id)
        {
            return null;
        }

        sourceMovement.MakeVariantOf(
            parentMovement.Id,
            SeedUser);

        return new SeedChange(
            $"{sourceMovement.NameKey}->variant-of->{parentMovement.NameKey}",
            SeedChangeType.Updated);
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
            .Include(movement => movement.EquipmentRequirements)
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