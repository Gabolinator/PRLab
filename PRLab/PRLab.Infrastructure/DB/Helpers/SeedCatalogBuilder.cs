using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding.Catalog;
using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Helpers;

public static class SeedCatalogBuilder
{
    public static async Task<EquipmentSeedCatalog> CreateEquipmentCatalog(
        PRLabPgDBContext db,
        CancellationToken ct)
    {
        var equipments = await db.Equipments
            .Include(equipment => equipment.Description)
                .ThenInclude(description => description.Translations)
            .ToListAsync(ct);

        return new EquipmentSeedCatalog(
            new EntityCatalog<EquipmentId, Equipment>(
                equipments,
                equipment => equipment.Id,
                equipment => equipment.NameKey));
    }

    public static async Task<MuscleSeedCatalog> CreateMuscleCatalog(
        PRLabPgDBContext db,
        CancellationToken ct)
    {
        var muscles = await db.Muscles
            .Include(muscle => muscle.Description)
                .ThenInclude(description => description.Translations)
            .Include(muscle => muscle.Antagonists)
            .ToListAsync(ct);

        return new MuscleSeedCatalog(
            new EntityCatalog<MuscleId, Muscle>(
                muscles,
                muscle => muscle.Id,
                muscle => muscle.NameKey));
    }

    public static async Task<MovementCategorySeedCatalog> CreateMovementCategoryCatalog(
        PRLabPgDBContext db,
        CancellationToken ct)
    {
        var movementCategories = await db.MovementCategories
            .Include(movementCategory => movementCategory.Description)
                .ThenInclude(description => description.Translations)
            .ToListAsync(ct);

        return new MovementCategorySeedCatalog(
            new EntityCatalog<MovementCategoryId, MovementCategory>(
                movementCategories,
                movementCategory => movementCategory.Id,
                movementCategory => movementCategory.NameKey));
    }

    public static async Task<MovementSeedCatalog> CreateMovementCatalog(
        PRLabPgDBContext db,
        CancellationToken ct)
    {
        var movements = await db.Movements
            .Include(movement => movement.Description)
                .ThenInclude(description => description.Translations)
            .Include(movement => movement.MovementCategory)
            .Include(movement => movement.Muscles)
            .Include(movement => movement.Equipments)
            .Include(movement => movement.Variants)
            .Include(movement => movement.VariantOf)
            .ToListAsync(ct);

        return new MovementSeedCatalog(
            new EntityCatalog<MovementId, Movement>(
                movements,
                movement => movement.Id,
                movement => movement.NameKey));
    }
}