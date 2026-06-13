using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;

namespace PRLab.Application.Interface.DB.Seeding.Factory.Entity.Exercise;

public interface IExerciseSeedFactory
{
    IReadOnlyList<SeedItem<Domain.Model.Entity.Exercise>> CreateInitialData(
        MovementSeedCatalog catalog);
}