using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories.Entity;

public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(ExerciseId id, CancellationToken ct);
    
    Task<Exercise?> GetTrackedByIdAsync(
        ExerciseId id,
        CancellationToken ct);

    Task<Exercise?> GetByNameAsync(string name, CancellationToken ct);

    Task<IReadOnlyCollection<Exercise>> ListAsync(CancellationToken ct);

    Task<IReadOnlyCollection<Exercise>> ListByMovementAsync(
        MovementId movementId,
        CancellationToken ct);

    Task<Exercise> CreateAsync(Exercise exercise, CancellationToken ct);

    Task<Exercise> UpdateAsync(Exercise exercise, CancellationToken ct);

    Task<bool> ExistsAsync(ExerciseId id, CancellationToken ct);

    Task<bool> NameExistsAsync(
        string name,
        ExerciseId? excludedExerciseId,
        CancellationToken ct);
}