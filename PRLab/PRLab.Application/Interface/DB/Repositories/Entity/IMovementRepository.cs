using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories.Entity;

public interface IMovementRepository
{
    Task<Movement?> GetByIdAsync(MovementId id, CancellationToken ct);

    Task<Movement?> GetByNameAsync(string name, CancellationToken ct);
    
    Task<Movement?> GetTrackedByNameAsync(
        string name,
        CancellationToken ct);
    
    Task<Movement?> GetTrackedByIdAsync(
        MovementId id,
        CancellationToken ct);

    Task<IReadOnlyCollection<Movement>> ListAsync(CancellationToken ct);

    Task<IReadOnlyCollection<Movement>> ListByCategoryAsync(
        MovementCategoryId movementCategoryId,
        CancellationToken ct);

    Task<IReadOnlyCollection<Movement>> ListByMuscleAsync(
        MuscleId muscleId,
        CancellationToken ct);

    Task<IReadOnlyCollection<Movement>> ListByEquipmentAsync(
        EquipmentId equipmentId,
        CancellationToken ct);

    Task<IReadOnlyCollection<Movement>> ListByMuscleRoleAsync(
        MuscleId muscleId,
        DomainEnum.MuscleRole role,
        CancellationToken ct);

    Task<IReadOnlyCollection<Movement>> ListVariantsAsync(
        MovementId movementId,
        CancellationToken ct);

    Task<Movement> CreateAsync(Movement movement, CancellationToken ct);

    Task<Movement> UpdateAsync(Movement movement, CancellationToken ct);

    Task<bool> ExistsAsync(MovementId id, CancellationToken ct);

    Task<bool> NameExistsAsync(string name, MovementId? excludedMovementId, CancellationToken ct);
}