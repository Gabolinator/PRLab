using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Enum.Anatomy;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class MovementRepository(PRLabPgDBContext db) : IMovementRepository
{
    public async Task<Movement?> GetByIdAsync(MovementId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(id));
        }

        return await BaseMovementReadQuery()
            .FirstOrDefaultAsync(
                movement => movement.Id == id,
                ct);
    }

    public async Task<Movement?> GetTrackedByNameAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Movement name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseMovementWriteQuery()
            .FirstOrDefaultAsync(
                movement => movement.NameKey == nameKey,
                ct);
    }

    public async Task<Movement?> GetTrackedByIdAsync(MovementId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(id));
        }

        return await BaseMovementWriteQuery()
            .FirstOrDefaultAsync(
                movement => movement.Id == id,
                ct);
    }

    public async Task<Movement?> GetByNameAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Movement name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseMovementReadQuery()
            .FirstOrDefaultAsync(
                movement => movement.NameKey == nameKey,
                ct);
    }

    public async Task<IReadOnlyCollection<Movement>> ListAsync(CancellationToken ct)
    {
        return await BaseMovementReadQuery()
            .OrderBy(movement => movement.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Movement>> ListByCategoryAsync(
        MovementCategoryId movementCategoryId,
        CancellationToken ct)
    {
        if (movementCategoryId.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement category id cannot be empty.", nameof(movementCategoryId));
        }

        return await BaseMovementReadQuery()
            .Where(movement => movement.MovementCategoryId == movementCategoryId)
            .OrderBy(movement => movement.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Movement>> ListByMuscleAsync(
        MuscleId muscleId,
        CancellationToken ct)
    {
        if (muscleId.Value == Guid.Empty)
        {
            throw new ArgumentException("Muscle id cannot be empty.", nameof(muscleId));
        }

        return await BaseMovementReadQuery()
            .Where(movement =>
                movement.Muscles.Any(movementMuscle => movementMuscle.MuscleId == muscleId))
            .OrderBy(movement => movement.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Movement>> ListByEquipmentAsync(
        EquipmentId equipmentId,
        CancellationToken ct)
    {
        if (equipmentId.Value == Guid.Empty)
        {
            throw new ArgumentException("Equipment id cannot be empty.", nameof(equipmentId));
        }

        return await BaseMovementReadQuery()
            .Where(movement =>
                movement.EquipmentRequirements.Any(requirement => requirement.EquipmentId == equipmentId))
            .OrderBy(movement => movement.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Movement>> ListByMuscleRoleAsync(
        MuscleId muscleId,
        MuscleRole role,
        CancellationToken ct)
    {
        if (muscleId.Value == Guid.Empty)
        {
            throw new ArgumentException("Muscle id cannot be empty.", nameof(muscleId));
        }

        return await BaseMovementReadQuery()
            .Where(movement =>
                movement.Muscles.Any(movementMuscle =>
                    movementMuscle.MuscleId == muscleId &&
                    movementMuscle.Role == role))
            .OrderBy(movement => movement.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Movement>> ListVariantsAsync(
        MovementId movementId,
        CancellationToken ct)
    {
        if (movementId.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(movementId));
        }

        return await BaseMovementReadQuery()
            .Where(movement => movement.VariantOfId == movementId)
            .OrderBy(movement => movement.Name)
            .ToListAsync(ct);
    }

    public async Task<Movement> CreateAsync(Movement movement, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(movement);

        await db.Movements.AddAsync(movement, ct);
        await db.SaveChangesAsync(ct);

        return movement;
    }

    public async Task<Movement> UpdateAsync(Movement movement, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(movement);

        if (movement.Id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(movement));
        }

        await db.SaveChangesAsync(ct);

        return movement;
    }

    public async Task<bool> ExistsAsync(MovementId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(id));
        }

        return await db.Movements
            .AsNoTracking()
            .AnyAsync(
                movement => movement.Id == id &&
                            !movement.Audit.IsDeleted,
                ct);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        MovementId? excludedMovementId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Movement name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await db.Movements
            .AsNoTracking()
            .AnyAsync(
                movement =>
                    movement.NameKey == nameKey &&
                    !movement.Audit.IsDeleted &&
                    (!excludedMovementId.HasValue || movement.Id != excludedMovementId.Value),
                ct);
    }

    private IQueryable<Movement> BaseMovementReadQuery()
    {
        return BaseMovementQuery()
            .AsNoTracking();
    }

    private IQueryable<Movement> BaseMovementWriteQuery()
    {
        return BaseMovementQuery();
    }

    private IQueryable<Movement> BaseMovementQuery()
    {
        return db.Movements
            .AsSplitQuery()
            .Include(movement => movement.Description)
                .ThenInclude(description => description.Translations)
            .Include(movement => movement.MovementCategory)
                .ThenInclude(movementCategory => movementCategory.Description)
                    .ThenInclude(description => description.Translations)
            .Include(movement => movement.VariantOf)
            .Include(movement => movement.Variants)
            .Include(movement => movement.Patterns)
            .Include(movement => movement.Muscles)
                .ThenInclude(movementMuscle => movementMuscle.Muscle)
                    .ThenInclude(muscle => muscle.Description)
                        .ThenInclude(description => description.Translations)
            .Include(movement => movement.EquipmentRequirements)
                .ThenInclude(requirement => requirement.Equipment)
                    .ThenInclude(equipment => equipment.Description)
                        .ThenInclude(description => description.Translations)
            .Where(movement => !movement.Audit.IsDeleted);
    }
}