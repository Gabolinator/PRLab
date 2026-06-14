using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public class EquipmentRepository(PRLabPgDBContext db, IClock clock) : IEquipmentRepository
{
    public async Task<Equipment?> GetByIdAsync(EquipmentId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Equipment id cannot be empty.", nameof(id));
        }

        return await BaseEquipmentQuery()
            .FirstOrDefaultAsync(
                equipment => equipment.Id == id,
                ct);
    }

    public async Task<Equipment?> GetByNameAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseEquipmentQuery()
            .FirstOrDefaultAsync(
                equipment => equipment.NameKey == nameKey,
                ct);
    }

    public async Task<IReadOnlyCollection<Equipment>> ListAsync(CancellationToken ct)
    {
        return await BaseEquipmentQuery()
            .OrderBy(equipment => equipment.Name)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        return await db.Equipments
            .AsNoTracking()
            .CountAsync(
                equipment => !equipment.Audit.IsDeleted,
                ct);
    }

    public async Task<Equipment> CreateAsync(Equipment equipment, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        await db.Equipments.AddAsync(equipment, ct);
        await db.SaveChangesAsync(ct);

        return equipment;
    }

    public async Task<Equipment> UpdateAsync(Equipment equipment, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        if (equipment.Id.Value == Guid.Empty)
        {
            throw new ArgumentException("Equipment id cannot be empty.", nameof(equipment));
        }

        db.Equipments.Update(equipment);
        await db.SaveChangesAsync(ct);

        return equipment;
    }

    public async Task<bool> ExistsAsync(EquipmentId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Equipment id cannot be empty.", nameof(id));
        }

        return await db.Equipments
            .AsNoTracking()
            .AnyAsync(
                equipment => equipment.Id == id &&
                             !equipment.Audit.IsDeleted,
                ct);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        EquipmentId? excludedEquipmentId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await db.Equipments
            .AsNoTracking()
            .AnyAsync(
                equipment =>
                    equipment.NameKey == nameKey &&
                    !equipment.Audit.IsDeleted &&
                    (!excludedEquipmentId.HasValue || equipment.Id != excludedEquipmentId.Value),
                ct);
    }

    private IQueryable<Equipment> BaseEquipmentQuery()
    {
        return db.Equipments
            .AsNoTracking()
            .Include(equipment => equipment.Description)
            .ThenInclude(description => description.Translations)
            .Where(equipment => !equipment.Audit.IsDeleted);
    }
}