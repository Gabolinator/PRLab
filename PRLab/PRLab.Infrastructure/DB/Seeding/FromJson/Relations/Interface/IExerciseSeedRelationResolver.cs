using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Exercise;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

public interface IExerciseSeedRelationResolver
{
    void ApplyRelations(
        Exercise exercise,
        ExerciseSeedJsonDto seedDto,
        MovementSeedCatalog catalog,
        User seedUser);
}