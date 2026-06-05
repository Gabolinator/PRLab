using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Models.DB.Seeding.Catalog;

public sealed class EquipmentSeedCatalog(
    EntityCatalog<EquipmentId, Equipment> catalog)
    : BaseSeedCatalog<EquipmentId, Equipment>(catalog)
{
}