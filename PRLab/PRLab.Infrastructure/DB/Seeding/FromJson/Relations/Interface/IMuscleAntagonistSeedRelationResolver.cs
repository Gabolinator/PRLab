using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

public interface IMuscleAntagonistSeedRelationResolver
{
    IReadOnlyList<SeedRelationItem<MuscleId>> Resolve(
        IReadOnlyCollection<MuscleSeedJsonDto> seedDtos,
        MuscleSeedCatalog muscleCatalog);
}