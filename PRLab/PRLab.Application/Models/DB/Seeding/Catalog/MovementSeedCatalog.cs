using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Models.DB.Seeding.Catalog;

public sealed class MovementSeedCatalog(
    EntityCatalog<MovementId, Movement> catalog)
    : BaseSeedCatalog<MovementId, Movement>(catalog)
{
}