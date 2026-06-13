using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories.Entity;

public interface IMovementCategoryRepository
{
    Task<MovementCategory?> GetByIdAsync(
        MovementCategoryId id,
        CancellationToken ct);

    Task<MovementCategory?> GetByNameAsync(string name, CancellationToken ct);

    Task<IReadOnlyCollection<MovementCategory>> ListAsync(CancellationToken ct);

    Task<IReadOnlyCollection<MovementCategory>> ListByBaseCategoryAsync(
        BaseMovementCategory baseMovementCategory,
        CancellationToken ct);

    Task<MovementCategory> CreateAsync(
        MovementCategory movementCategory,
        CancellationToken ct);

    Task<MovementCategory> UpdateAsync(
        MovementCategory movementCategory,
        CancellationToken ct);

    Task<bool> ExistsAsync(MovementCategoryId id, CancellationToken ct);

    Task<bool> NameExistsAsync(
        string name,
        MovementCategoryId? excludedMovementCategoryId,
        CancellationToken ct);
}