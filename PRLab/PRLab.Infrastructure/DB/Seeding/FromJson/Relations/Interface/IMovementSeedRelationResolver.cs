using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

public interface IMovementSeedRelationResolver
{
    void ApplyRelations(
        Movement movement,
        MovementSeedJsonDto seedDto,
        MovementSeedCatalogs catalogs,
        User seedUser);
}