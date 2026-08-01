using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Application.Models.DB.Querying;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public class WorkoutRepository(PRLabPgDBContext db) : IWorkoutRepository
{
    public Task<Workout?> GetByIdAsync(WorkoutId id, CancellationToken ct)
    {
        DomainGuard.NotEmptyId(id, nameof(WorkoutId));
        
        return BaseWorkoutReadQuery()
            .FirstOrDefaultAsync(workout=> workout.Id == id, ct);
    }

    public async Task<Workout?> GetByNameAsync(string name, CancellationToken ct)
    {
        DomainGuard.NotEmptyName(name, nameof(Workout));

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseWorkoutReadQuery()
            .FirstOrDefaultAsync(
                movement => movement.NameKey == nameKey,
                ct);
    }

    public async Task<Workout?> GetTrackedByNameAsync(string name, CancellationToken ct)
    {
        DomainGuard.NotEmptyName(name, nameof(Workout));

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseWorkoutWriteQuery()
            .FirstOrDefaultAsync(
                movement => movement.NameKey == nameKey,
                ct);
    }

    public Task<Workout?> GetTrackedByIdAsync(WorkoutId id, CancellationToken ct)
    {
        DomainGuard.NotEmptyId(id, nameof(WorkoutId));
        
        return BaseWorkoutWriteQuery()
            .FirstOrDefaultAsync(workout=> workout.Id == id, ct);
    }

    public async Task<IReadOnlyCollection<Workout>> ListAsync(CancellationToken ct)
    {
       return await BaseWorkoutReadQuery()
           .OrderBy(workout => workout.Name)
           .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Workout>> ListAvailableToUserAsync(UserId userId, CancellationToken ct)
    {
        // get all public + owned by user - IsVisibleToUser just checks IsPublic visibility for now 
        var ownedByUser = await ListByAuthorIdAsync(userId, ct);
        var publicWorkouts =  await BaseWorkoutReadQuery()
            .Where(workout => workout.Visibility.IsVisibleToUser(userId))
            .OrderBy(workout => workout.Name)
            .ToListAsync(ct);
        
        return ownedByUser.Concat(publicWorkouts).DistinctBy(workout => workout.Id).ToList();
    }

    public async Task<IReadOnlyList<Workout>> ListByAuthorIdAsync(UserId userId, CancellationToken ct)
    {
       return await BaseWorkoutReadQuery()
            .Where(workout => workout.Ownership.IsOwnedById(userId))
            .OrderBy(workout => workout.Name)
            .ToListAsync(ct);
    }

    public async Task<Workout> CreateAsync(Workout workout, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(workout);
       
        await db.Workouts.AddAsync(workout, ct);
        await db.SaveChangesAsync(ct);

        return workout;
    }

    public async Task<Workout> UpdateAsync(Workout workout, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(workout);

        DomainGuard.ValidRequiredId(workout.Id, nameof(Movement));

        await db.SaveChangesAsync(ct);

        return workout;
    }

    public Task<bool> ExistsAsync(WorkoutId id, CancellationToken ct)
    {
        DomainGuard.NotEmptyId(id, nameof(WorkoutId));
        
        return BaseWorkoutReadQuery()
            .AnyAsync(workout => workout.Id == id, ct);
    }

    public Task<bool> NameExistsAsync(string name, WorkoutId? excludedWorkoutId, CancellationToken ct)
    {
        DomainGuard.NotEmptyName(name, nameof(Workout));
        
        var nameKey = FormatingUtilities.NormalizeNameKey(name);
        
        return BaseWorkoutReadQuery()
            .AnyAsync(
                workout =>
                    workout.NameKey == nameKey &&
                    !workout.Audit.IsDeleted &&
                    (!excludedWorkoutId.HasValue || workout.Id != excludedWorkoutId.Value),
                ct);
    }

    public Task<IReadOnlyList<Workout>> SearchAsync(WorkoutSearchQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
    
    private IQueryable<Workout> BaseWorkoutReadQuery()
    {
        return db.Workouts.ForFullRead();
    }

    private IQueryable<Workout> BaseWorkoutWriteQuery()
    {
        return db.Workouts.ForFullWrite();
    }
}