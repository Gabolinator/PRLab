using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Application.Interface.DB.Seeding.Factory.Entity.Muscle;

public interface IMuscleSeedFactory
{
    public IReadOnlyList<SeedItem<Domain.Model.Entity.Muscle>> CreateInitialData();
}