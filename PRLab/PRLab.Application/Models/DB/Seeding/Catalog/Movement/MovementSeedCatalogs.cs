namespace PRLab.Application.Models.DB.Seeding.Catalog.Movement;

public sealed class MovementSeedCatalogs(
    EquipmentSeedCatalog equipmentCatalog,
    MuscleSeedCatalog muscleCatalog,
    MovementCategorySeedCatalog movementCategoryCatalog,
    MovementSeedCatalog? movementCatalog = null)
{
    public EquipmentSeedCatalog Equipment { get; } = equipmentCatalog;

    public MuscleSeedCatalog Muscle { get; } = muscleCatalog;

    public MovementCategorySeedCatalog MovementCategory { get; } = movementCategoryCatalog;

    public MovementSeedCatalog? Movement { get; } = movementCatalog;
}