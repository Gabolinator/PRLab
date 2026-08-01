using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class ExerciseRepository(
    PRLabPgDBContext db) : IExerciseRepository
{
    public async Task<Exercise?> GetByIdAsync(
        ExerciseId id,
        CancellationToken ct)
    {
        DomainGuard.NotEmptyId(
            id,
            nameof(id));

        return await BaseExerciseReadQuery()
            .FirstOrDefaultAsync(
                exercise => exercise.Id == id,
                ct);
    }

    public async Task<Exercise?> GetTrackedByIdAsync(
        ExerciseId id,
        CancellationToken ct)
    {
        DomainGuard.NotEmptyId(
            id,
            nameof(id));

        return await BaseExerciseWriteQuery()
            .FirstOrDefaultAsync(
                exercise => exercise.Id == id,
                ct);
    }

    public async Task<Exercise?> GetByNameAsync(
        string name,
        CancellationToken ct)
    {
        DomainGuard.NotEmptyName(
            name,
            nameof(name));

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseExerciseReadQuery()
            .FirstOrDefaultAsync(
                exercise => exercise.NameKey == nameKey,
                ct);
    }

    public async Task<IReadOnlyCollection<Exercise>> ListAsync(
        CancellationToken ct)
    {
        return await BaseExerciseReadQuery()
            .OrderBy(exercise => exercise.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Exercise>> ListByMovementAsync(
        MovementId movementId,
        CancellationToken ct)
    {
        DomainGuard.NotEmptyId(
            movementId,
            nameof(movementId));

        return await BaseExerciseReadQuery()
            .Where(exercise =>
                exercise.Steps.Any(
                    exerciseStep =>
                        exerciseStep.MovementId == movementId))
            .OrderBy(exercise => exercise.Name)
            .ToListAsync(ct);
    }

    public async Task<Exercise> CreateAsync(
        Exercise exercise,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        DomainGuard.NotEmptyId(
            exercise.Id,
            nameof(exercise.Id));

        await db.Exercises.AddAsync(
            exercise,
            ct);

        await db.SaveChangesAsync(ct);

        return exercise;
    }

    public async Task<Exercise> UpdateAsync(
        Exercise exercise,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        DomainGuard.NotEmptyId(
            exercise.Id,
            nameof(exercise.Id));

        if (db.Entry(exercise).State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                "Exercise must be loaded with GetTrackedByIdAsync " +
                "before it can be updated.");
        }

        await db.SaveChangesAsync(ct);

        return exercise;
    }

    public async Task<bool> ExistsAsync(
        ExerciseId id,
        CancellationToken ct)
    {
        DomainGuard.NotEmptyId(
            id,
            nameof(id));

        return await BaseExerciseLookupQuery()
            .AnyAsync(
                exercise => exercise.Id == id,
                ct);
    }

    public Task<bool> NameExistsAsync(
        string name,
        CancellationToken ct)
    {
        return NameExistsAsync(
            name,
            excludedExerciseId: null,
            ct);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        ExerciseId? excludedExerciseId,
        CancellationToken ct)
    {
        DomainGuard.NotEmptyName(
            name,
            nameof(name));

        if (excludedExerciseId.HasValue)
        {
            DomainGuard.NotEmptyId(
                excludedExerciseId.Value,
                nameof(excludedExerciseId));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseExerciseLookupQuery()
            .AnyAsync(
                exercise =>
                    exercise.NameKey == nameKey &&
                    (!excludedExerciseId.HasValue ||
                     exercise.Id != excludedExerciseId.Value),
                ct);
    }

    private IQueryable<Exercise> BaseExerciseReadQuery()
    {
        return db.Exercises
            .ForFullRead();
    }

    private IQueryable<Exercise> BaseExerciseWriteQuery()
    {
        return db.Exercises
            .ForFullWrite();
    }

    private IQueryable<Exercise> BaseExerciseLookupQuery()
    {
        return db.Exercises
            .ActiveOnly()
            .AsNoTracking();
    }
}