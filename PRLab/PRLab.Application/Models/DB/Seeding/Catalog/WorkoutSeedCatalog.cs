using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Models.DB.Seeding.Catalog;

public class WorkoutSeedCatalog(EntityCatalog<WorkoutId, Domain.Model.Entity.Workout> catalog)
    : BaseSeedCatalog<WorkoutId, Domain.Model.Entity.Workout>(catalog)
{
    
}