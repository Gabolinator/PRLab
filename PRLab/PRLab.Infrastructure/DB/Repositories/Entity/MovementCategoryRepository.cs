using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class MovementCategoryRepository(
    PRLabPgDBContext db) : IMovementCategoryRepository
{
    public async Task<MovementCategory?> GetByIdAsync(
        MovementCategoryId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseMovementCategoryReadQuery()
            .FirstOrDefaultAsync(
                movementCategory => movementCategory.Id == id,
                ct);
    }

    public async Task<MovementCategory?> GetTrackedByIdAsync(
        MovementCategoryId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseMovementCategoryWriteQuery()
            .FirstOrDefaultAsync(
                movementCategory => movementCategory.Id == id,
                ct);
    }

    public async Task<MovementCategory?> GetByNameAsync(
        string name,
        CancellationToken ct)
    {
        ValidateName(name);

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseMovementCategoryReadQuery()
            .FirstOrDefaultAsync(
                movementCategory =>
                    movementCategory.NameKey == nameKey,
                ct);
    }

    public async Task<IReadOnlyCollection<MovementCategory>> ListAsync(
        CancellationToken ct)
    {
        return await BaseMovementCategoryReadQuery()
            .OrderBy(movementCategory => movementCategory.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<MovementCategory>>
        ListByBaseCategoryAsync(
            BaseMovementCategory baseMovementCategory,
            CancellationToken ct)
    {
        ValidateBaseMovementCategory(baseMovementCategory);

        return await BaseMovementCategoryReadQuery()
            .Where(movementCategory =>
                movementCategory.BaseMovementCategory ==
                baseMovementCategory)
            .OrderBy(movementCategory => movementCategory.Name)
            .ToListAsync(ct);
    }

    public async Task<MovementCategory> CreateAsync(
        MovementCategory movementCategory,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(movementCategory);

        ValidateId(movementCategory.Id);

        await db.MovementCategories.AddAsync(
            movementCategory,
            ct);

        await db.SaveChangesAsync(ct);

        return movementCategory;
    }

    public async Task<MovementCategory> UpdateAsync(
        MovementCategory movementCategory,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(movementCategory);

        ValidateId(movementCategory.Id);

        if (db.Entry(movementCategory).State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                "Movement category must be loaded with " +
                "GetTrackedByIdAsync before it can be updated.");
        }

        await db.SaveChangesAsync(ct);

        return movementCategory;
    }

    public async Task<bool> ExistsAsync(
        MovementCategoryId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseMovementCategoryLookupQuery()
            .AnyAsync(
                movementCategory => movementCategory.Id == id,
                ct);
    }

    public Task<bool> NameExistsAsync(
        string name,
        CancellationToken ct)
    {
        return NameExistsAsync(
            name,
            excludedMovementCategoryId: null,
            ct);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        MovementCategoryId? excludedMovementCategoryId,
        CancellationToken ct)
    {
        ValidateName(name);

        if (excludedMovementCategoryId.HasValue)
        {
            ValidateId(excludedMovementCategoryId.Value);
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseMovementCategoryLookupQuery()
            .AnyAsync(
                movementCategory =>
                    movementCategory.NameKey == nameKey &&
                    (!excludedMovementCategoryId.HasValue ||
                     movementCategory.Id !=
                     excludedMovementCategoryId.Value),
                ct);
    }

    private IQueryable<MovementCategory>
        BaseMovementCategoryReadQuery()
    {
        return db.MovementCategories
            .ForFullRead();
    }

    private IQueryable<MovementCategory>
        BaseMovementCategoryWriteQuery()
    {
        return db.MovementCategories
            .ForFullWrite();
    }

    private IQueryable<MovementCategory>
        BaseMovementCategoryLookupQuery()
    {
        return db.MovementCategories
            .ActiveOnly()
            .AsNoTracking();
    }

    private static void ValidateId(
        MovementCategoryId id)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Movement category id cannot be empty.",
                nameof(id));
        }
    }
    
    private static void ValidateName(string name)
        => DomainGuard.NotEmptyName(name, nameof(name));

    private static void ValidateBaseMovementCategory(
        BaseMovementCategory baseMovementCategory)
    {
        if (!Enum.IsDefined(baseMovementCategory))
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseMovementCategory),
                baseMovementCategory,
                "Unsupported base movement category.");
        }
    }
}