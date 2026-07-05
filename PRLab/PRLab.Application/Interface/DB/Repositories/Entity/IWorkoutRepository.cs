using PRLab.Application.Models.DB.Querying;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories.Entity;

public interface IWorkoutRepository
{
    Task<Workout?> GetByIdAsync(WorkoutId id, CancellationToken ct);

    Task<Workout?> GetByNameAsync(string name, CancellationToken ct);
    
    Task<Workout?> GetTrackedByNameAsync(
        string name,
        CancellationToken ct);
    
    Task<Workout?> GetTrackedByIdAsync(
        WorkoutId id,
        CancellationToken ct);
    
    Task<IReadOnlyCollection<Workout>> ListAsync(CancellationToken ct);
    
    Task<IReadOnlyList<Workout>> ListAvailableToUserAsync(UserId userId, CancellationToken ct);
    
    Task<IReadOnlyList<Workout>> ListByOwnerIdAsync(UserId userId, CancellationToken ct);
    
    Task<IReadOnlyList<Workout>> ListByAuthorIdAsync(UserId userId, CancellationToken ct);
    Task<Workout> CreateAsync(Workout workout, CancellationToken ct);

    Task<Workout> UpdateAsync(Workout workout, CancellationToken ct);

    Task<bool> ExistsAsync(WorkoutId id, CancellationToken ct);

    Task<bool> NameExistsAsync(string name, WorkoutId? excludedWorkoutId, CancellationToken ct);
    
    Task<IReadOnlyList<Workout>> SearchAsync(WorkoutSearchQuery query, CancellationToken ct);
}