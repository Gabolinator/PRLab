using PRLab.Domain.Model.Entity;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IMuscleSeedFactory
{
    public IReadOnlyList<SeedItem<Muscle>> CreateInitialData();
}