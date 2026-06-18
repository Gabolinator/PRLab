using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Application.Interface.DB.Seeding.Factory.Entity.Workout;

public interface IWorkoutSeedFactory
{
    public IReadOnlyList<SeedItem<Domain.Model.Entity.Workout>> CreateInitialData();
}