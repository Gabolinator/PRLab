using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class ExerciseRepository(PRLabPgDBContext db) : IExerciseRepository
{
    public async Task<Exercise?> GetByIdAsync(ExerciseId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Exercise id cannot be empty.", nameof(id));
        }

        return await BaseExerciseReadQuery()
            .FirstOrDefaultAsync(
                exercise => exercise.Id == id,
                ct);
    }

    public async Task<Exercise?> GetTrackedByIdAsync(ExerciseId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Exercise id cannot be empty.", nameof(id));
        }

        return await BaseExerciseWriteQuery()
            .FirstOrDefaultAsync(
                exercise => exercise.Id == id,
                ct);
    }

    public async Task<Exercise?> GetByNameAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Exercise name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseExerciseReadQuery()
            .FirstOrDefaultAsync(
                exercise => exercise.NameKey == nameKey,
                ct);
    }

    public async Task<IReadOnlyCollection<Exercise>> ListAsync(CancellationToken ct)
    {
        return await BaseExerciseReadQuery()
            .OrderBy(exercise => exercise.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Exercise>> ListByMovementAsync(
        MovementId movementId,
        CancellationToken ct)
    {
        if (movementId.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(movementId));
        }

        return await BaseExerciseReadQuery()
            .Where(exercise => exercise.Steps.Any(block => block.MovementId == movementId))
            .OrderBy(exercise => exercise.Name)
            .ToListAsync(ct);
    }

    public async Task<Exercise> CreateAsync(Exercise exercise, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        await db.Exercises.AddAsync(exercise, ct);
        await db.SaveChangesAsync(ct);

        return exercise;
    }

    public async Task<Exercise> UpdateAsync(Exercise exercise, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        if (exercise.Id.Value == Guid.Empty)
        {
            throw new ArgumentException("Exercise id cannot be empty.", nameof(exercise));
        }

        await db.SaveChangesAsync(ct);

        return exercise;
    }

    public async Task<bool> ExistsAsync(ExerciseId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Exercise id cannot be empty.", nameof(id));
        }

        return await db.Exercises
            .AsNoTracking()
            .AnyAsync(
                exercise => exercise.Id == id &&
                            !exercise.Audit.IsDeleted,
                ct);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        ExerciseId? excludedExerciseId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Exercise name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await db.Exercises
            .AsNoTracking()
            .AnyAsync(
                exercise =>
                    exercise.NameKey == nameKey &&
                    !exercise.Audit.IsDeleted &&
                    (!excludedExerciseId.HasValue || exercise.Id != excludedExerciseId.Value),
                ct);
    }

    private IQueryable<Exercise> BaseExerciseReadQuery()
    {
        return BaseExerciseQuery()
            .AsNoTracking();
    }

    private IQueryable<Exercise> BaseExerciseWriteQuery()
    {
        return BaseExerciseQuery();
    }

    private IQueryable<Exercise> BaseExerciseQuery()
    {
        return db.Exercises
            .AsSplitQuery()
            .Include(exercise => exercise.Description)
                .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
                .ThenInclude(block => block.Movement)
                    .ThenInclude(movement => movement.Description)
                        .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
                .ThenInclude(block => block.Movement)
                    .ThenInclude(movement => movement.MovementCategory)
                        .ThenInclude(movementCategory => movementCategory.Description)
                            .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
                .ThenInclude(block => block.Movement)
                    .ThenInclude(movement => movement.Patterns)
            .Include(exercise => exercise.Steps)
                .ThenInclude(block => block.Movement)
                    .ThenInclude(movement => movement.Muscles)
                        .ThenInclude(movementMuscle => movementMuscle.Muscle)
                            .ThenInclude(muscle => muscle.Description)
                                .ThenInclude(description => description.Translations)
            .Include(exercise => exercise.Steps)
                .ThenInclude(block => block.Movement)
                    .ThenInclude(movement => movement.EquipmentRequirements)
                        .ThenInclude(requirement => requirement.Equipment)
                            .ThenInclude(equipment => equipment.Description)
                                .ThenInclude(description => description.Translations)
            .Where(exercise => !exercise.Audit.IsDeleted);
    }
}