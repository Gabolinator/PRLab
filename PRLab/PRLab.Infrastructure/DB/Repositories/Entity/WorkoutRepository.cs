using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Application.Models.DB.Querying;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public class WorkoutRepository : IWorkoutRepository
{
    public Task<Workout?> GetByIdAsync(WorkoutId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Workout?> GetByNameAsync(string name, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Workout?> GetTrackedByNameAsync(string name, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Workout?> GetTrackedByIdAsync(WorkoutId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<Workout>> ListAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Workout>> ListAvailableToUserAsync(UserId userId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Workout>> ListByOwnerIdAsync(UserId userId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Workout>> ListByAuthorIdAsync(UserId userId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Workout> CreateAsync(Workout workout, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Workout> UpdateAsync(Workout workout, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(WorkoutId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<bool> NameExistsAsync(string name, WorkoutId? excludedWorkoutId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Workout>> SearchAsync(WorkoutSearchQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}