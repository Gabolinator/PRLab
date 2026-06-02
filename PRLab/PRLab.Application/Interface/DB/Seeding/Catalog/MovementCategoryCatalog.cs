using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Seeding.Catalog;

public sealed class MovementCategorySeedCatalog(
    EntityCatalog<MovementCategoryId, MovementCategory> catalog)
    : BaseSeedCatalog<MovementCategoryId, MovementCategory>(catalog)
{
}