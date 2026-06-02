using PRLab.Application.Interface.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IMovementSeedFactory
{
    IReadOnlyList<SeedItem<Movement>> CreateInitialData(
        EquipmentSeedCatalog equipmentCatalog,
        MuscleSeedCatalog muscleCatalog,
        MovementCategorySeedCatalog movementCategoryCatalog);
}