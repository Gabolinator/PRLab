using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

public interface IWorkoutSeedRelationResolver
{
    void ApplyRelations(
        Workout exercise,
        WorkoutSeedJsonDto seedDto,
        ExerciseSeedCatalog catalog,
        User seedUser);
}
