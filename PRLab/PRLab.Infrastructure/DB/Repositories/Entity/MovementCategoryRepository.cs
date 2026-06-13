using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class MovementCategoryRepository(PRLabPgDBContext db) : IMovementCategoryRepository
{
    public async Task<MovementCategory?> GetByIdAsync(
        MovementCategoryId id,
        CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement category id cannot be empty.", nameof(id));
        }

        return await BaseMovementCategoryQuery()
            .FirstOrDefaultAsync(
                movementCategory => movementCategory.Id == id,
                ct);
    }

    public async Task<MovementCategory?> GetByNameAsync(
        string name,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Movement category name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseMovementCategoryQuery()
            .FirstOrDefaultAsync(
                movementCategory => movementCategory.NameKey == nameKey,
                ct);
    }

    public async Task<IReadOnlyCollection<MovementCategory>> ListAsync(
        CancellationToken ct)
    {
        return await BaseMovementCategoryQuery()
            .OrderBy(movementCategory => movementCategory.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<MovementCategory>> ListByBaseCategoryAsync(
        BaseMovementCategory baseMovementCategory,
        CancellationToken ct)
    {
        return await BaseMovementCategoryQuery()
            .Where(movementCategory => movementCategory.BaseMovementCategory == baseMovementCategory)
            .OrderBy(movementCategory => movementCategory.Name)
            .ToListAsync(ct);
    }

    public async Task<MovementCategory> CreateAsync(
        MovementCategory movementCategory,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(movementCategory);

        await db.MovementCategories.AddAsync(movementCategory, ct);
        await db.SaveChangesAsync(ct);

        return movementCategory;
    }

    public async Task<MovementCategory> UpdateAsync(
        MovementCategory movementCategory,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(movementCategory);

        if (movementCategory.Id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Movement category id cannot be empty.",
                nameof(movementCategory));
        }

        db.MovementCategories.Update(movementCategory);
        await db.SaveChangesAsync(ct);

        return movementCategory;
    }

    public async Task<bool> ExistsAsync(
        MovementCategoryId id,
        CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Movement category id cannot be empty.", nameof(id));
        }

        return await db.MovementCategories
            .AsNoTracking()
            .AnyAsync(
                movementCategory =>
                    movementCategory.Id == id &&
                    !movementCategory.Audit.IsDeleted,
                ct);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        CancellationToken ct)
    {
        return await NameExistsAsync(
            name,
            null,
            ct);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        MovementCategoryId? excludedMovementCategoryId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Movement category name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await db.MovementCategories
            .AsNoTracking()
            .AnyAsync(
                movementCategory =>
                    movementCategory.NameKey == nameKey &&
                    !movementCategory.Audit.IsDeleted &&
                    (
                        !excludedMovementCategoryId.HasValue ||
                        movementCategory.Id != excludedMovementCategoryId.Value
                    ),
                ct);
    }

    private IQueryable<MovementCategory> BaseMovementCategoryQuery()
    {
        return db.MovementCategories
            .AsNoTracking()
            .Include(movementCategory => movementCategory.Description)
                .ThenInclude(description => description.Translations)
            .Where(movementCategory => !movementCategory.Audit.IsDeleted);
    }
}