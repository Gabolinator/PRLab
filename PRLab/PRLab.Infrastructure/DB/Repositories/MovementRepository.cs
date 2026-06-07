using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories;

public sealed class MovementRepository(PRLabPgDBContext db) : IMovementRepository
{
    public async Task<Movement?> GetByIdAsync(MovementId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement id cannot be empty.", nameof(id));
        }

        return await CreateMovementQuery()
            .FirstOrDefaultAsync(
                movement => movement.Id == id && !movement.Audit.IsDeleted,
                ct);
    }

    public async Task<Movement?> GetByNameAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Movement name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await CreateMovementQuery()
            .FirstOrDefaultAsync(
                movement => movement.NameKey == nameKey && !movement.Audit.IsDeleted,
                ct);
    }

    public async Task<IReadOnlyCollection<Movement>> ListAsync(CancellationToken ct)
    {
        return await CreateMovementQuery()
            .AsNoTracking()
            .Where(movement => !movement.Audit.IsDeleted)
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

        return await CreateMovementQuery()
            .AsNoTracking()
            .Where(movement =>
                movement.MovementCategoryId == movementCategoryId &&
                !movement.Audit.IsDeleted)
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

        return await CreateMovementQuery()
            .AsNoTracking()
            .Where(movement =>
                movement.Muscles.Any(movementMuscle => movementMuscle.MuscleId == muscleId) &&
                !movement.Audit.IsDeleted)
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

        return await CreateMovementQuery()
            .AsNoTracking()
            .Where(movement =>
                movement.EquipmentRequirements.Any(requirement => requirement.EquipmentId == equipmentId) &&
                !movement.Audit.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Movement>> ListByMuscleRoleAsync(
        MuscleId muscleId,
        DomainEnum.MuscleRole role,
        CancellationToken ct)
    {
        if (muscleId.Value == Guid.Empty)
        {
            throw new ArgumentException("Muscle id cannot be empty.", nameof(muscleId));
        }

        return await CreateMovementQuery()
            .AsNoTracking()
            .Where(movement =>
                movement.Muscles.Any(movementMuscle =>
                    movementMuscle.MuscleId == muscleId &&
                    movementMuscle.Role == role) &&
                !movement.Audit.IsDeleted)
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

        return await CreateMovementQuery()
            .AsNoTracking()
            .Where(movement =>
                movement.VariantOfId == movementId &&
                !movement.Audit.IsDeleted)
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

        db.Movements.Update(movement);
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
                movement => movement.Id == id && !movement.Audit.IsDeleted,
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

    private IQueryable<Movement> CreateMovementQuery() => 
        db.Movements
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
                        .ThenInclude(description => description.Translations);
    
}