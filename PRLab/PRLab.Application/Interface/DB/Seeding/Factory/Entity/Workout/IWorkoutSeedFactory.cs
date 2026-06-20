using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;

namespace PRLab.Application.Interface.DB.Seeding.Factory.Entity.Workout;

public interface IWorkoutSeedFactory
{
    public IReadOnlyList<SeedItem<Domain.Model.Entity.Workout>> CreateInitialData(ExerciseSeedCatalog catalog);
}