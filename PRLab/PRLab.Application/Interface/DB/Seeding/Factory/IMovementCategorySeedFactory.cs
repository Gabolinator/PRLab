using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB.Seeding.Factory;

public interface IMovementCategorySeedFactory
{
    public IReadOnlyList<SeedItem<MovementCategory>> CreateInitialData();
}