using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB.Seeding.Factory;

public interface IMovementSeedFactory
{
    IReadOnlyList<SeedItem<Movement>> CreateInitialData(
        MovementSeedCatalogs catalogs);
}