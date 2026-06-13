using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Models.DB.Seeding.Catalog;

public class ExerciseSeedCatalog(
    EntityCatalog<ExerciseId, Domain.Model.Entity.Exercise> catalog)
    : BaseSeedCatalog<ExerciseId, Domain.Model.Entity.Exercise>(catalog)
{
}