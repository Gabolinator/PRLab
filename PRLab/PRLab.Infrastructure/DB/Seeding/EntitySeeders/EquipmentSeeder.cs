using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Update;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class EquipmentSeeder(
    PRLabPgDBContext db,
    IUserService userService, 
    IEquipmentSeedFactory seedFactory) : EntitySeederBase(db)
{
    public override int Order => SeedPolicy.GetSeedOrder(DomainEnum.EntityType.Equipment);
    public override string Name => "DevelopmentEquipmentSeed";

    public override string Version => "1.0.0";

    public override User SeedUser => userService.GetAdminUser("Seed");

    protected override async Task SeedEntityAsync(CancellationToken ct)
    {
        var equipmentSeedItems = seedFactory.CreateInitialData();

        foreach (var equipmentSeedItem in equipmentSeedItems)
        {
            await ApplyEquipmentSeedItem(equipmentSeedItem, ct);
        }
    }

    private async Task<Equipment?> ApplyEquipmentSeedItem(
        SeedItem<Equipment> equipmentSeedItem,
        CancellationToken ct)
    {
        if (equipmentSeedItem.Action == SeedAction.Ignore)
        {
            return null;
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

            return seedEquipment;
        }

        if (equipmentSeedItem.Action == SeedAction.CreateIfMissing)
        {
            return existingEquipment;
        }

        existingEquipment.Update(
            EquipmentUpdate.FromEquipment(
                seedEquipment,
                null,
                SeedUser));

        return existingEquipment;
    }
}