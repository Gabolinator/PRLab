using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Seeding.Factory.Entity;

public interface IMuscleAntagonistSeedFactory
{
    IReadOnlyList<SeedRelationItem<MuscleId>> CreateInitialData(MuscleSeedCatalog muscleCatalog);
}