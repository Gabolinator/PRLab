using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Seeding.Factory.Movement;

public interface IMovementVariantSeedFactory
{
    IReadOnlyList<SeedRelationItem<MovementId>> CreateVariantInitialData(
        MovementSeedCatalog movementCatalog);
}