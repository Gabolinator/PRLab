using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB.Seeding.Factory.Entity;

public interface IEquipmentSeedFactory
{
    public IReadOnlyList<SeedItem<Equipment>> CreateInitialData();
}