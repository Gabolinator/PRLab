using PRLab.Application.Interface.DB.Seeding.Catalog;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IMuscleAntagonistSeedFactory
{
    IReadOnlyList<SeedRelationItem<MuscleId>> CreateInitialData(MuscleSeedCatalog muscleCatalog);
}