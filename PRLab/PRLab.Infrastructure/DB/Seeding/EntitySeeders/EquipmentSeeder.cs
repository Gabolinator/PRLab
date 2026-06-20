using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class EquipmentSeeder(
    PRLabPgDBContext db,
    IUserService userService, 
    IEquipmentSeedFactory seedFactory,
    IAppLogger logger) : EntitySeederBase(db, logger)
{
    public override EntityType EntityType => EntityType.Equipment;
    public override string Name => "DevelopmentEquipmentSeed";

    public override string Version => "1.0.1";

    public override User SeedUser => userService.GetSystemAdminUser("Seed");

    protected override async Task<IReadOnlyList<SeedChange>> SeedEntityAsync(CancellationToken ct)
    {
        var equipmentSeedItems = seedFactory.CreateInitialData();
        var changes = new List<SeedChange>();

        foreach (var equipmentSeedItem in equipmentSeedItems)
        {
            var result = await ApplyEquipmentSeedItem(equipmentSeedItem, ct);

            if (result.change is not null)
            {
                changes.Add(result.change);
            }
        }

        return changes;
    }

    private async Task<(Equipment? entity, SeedChange? change)> ApplyEquipmentSeedItem(
        SeedItem<Equipment> equipmentSeedItem,
        CancellationToken ct)
    {
        if (equipmentSeedItem.Action == SeedAction.Ignore)
        {
            return (null, null);
        }

        var seedEquipment = equipmentSeedItem.Entity;

        var existingEquipment = await db.Equipments
            .Include(equipment => equipment.Description)
            .ThenInclude(description => description.Translations)
            .FirstOrDefaultAsync(
                equipment => equipment.NameKey == seedEquipment.NameKey,
                ct);

        if (existingEquipment is null)
        {
            await db.Equipments.AddAsync(seedEquipment, ct);

            logger.Log($"Seeded - {EntityType} : {seedEquipment.NameKey}");

            return (
                seedEquipment,
                new SeedChange(seedEquipment.NameKey, SeedChangeType.Created));
        }

        if (equipmentSeedItem.Action == SeedAction.CreateIfMissing)
        {
            return (existingEquipment, null);
        }

        logger.Log($"Seeder Updating - {EntityType} : {seedEquipment.NameKey}");

        var hasChanged = existingEquipment.Update(
            EquipmentUpdate.FromEquipment(
                seedEquipment,
                null,
                SeedUser));

        return hasChanged
            ? (existingEquipment, new SeedChange(seedEquipment.NameKey, SeedChangeType.Updated))
            : (existingEquipment, null);
    }
}