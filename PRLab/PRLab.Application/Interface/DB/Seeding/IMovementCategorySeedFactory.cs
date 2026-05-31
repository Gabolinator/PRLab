using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IMovementCategorySeedFactory
{
    public IReadOnlyList<SeedItem<MovementCategory>> CreateInitialData();
}