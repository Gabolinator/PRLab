using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Models.DB.Seeding.Catalog.Movement;

public sealed class MovementSeedCatalog(
    EntityCatalog<MovementId, Domain.Model.Entity.Movement> catalog)
    : BaseSeedCatalog<MovementId, Domain.Model.Entity.Movement>(catalog)
{
}