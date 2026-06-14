using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Models.DB.Seeding.Catalog;

public sealed class MuscleSeedCatalog(EntityCatalog<MuscleId, Muscle> catalog) : BaseSeedCatalog<MuscleId, Muscle>(catalog)
{
}