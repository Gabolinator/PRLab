using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IEquipmentSeedFactory
{
    public IReadOnlyList<SeedItem<Equipment>> CreateInitialData();
}