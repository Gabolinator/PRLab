using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;

namespace PRLab.Application.Interface.DB.Seeding.Factory.Movement;

public interface IMovementSeedFactory
{
    IReadOnlyList<SeedItem<Domain.Model.Entity.Movement>> CreateInitialData(
        MovementSeedCatalogs catalogs);
}