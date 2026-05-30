using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories;

public class EquipmentRepository(PRLabPgDBContext db, IClock clock) : IEquipmentRepository
{
    public async Task<Equipment?> GetByIdAsync(EquipmentId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Equipment id cannot be empty.", nameof(id));
        }

        return await db.Equipments
            .AsNoTracking()
            .Include(equipment => equipment.Description)
            .ThenInclude(description => description.Translations)
            .FirstOrDefaultAsync(equipment => equipment.Id == id, ct);
    }

    public async Task<Equipment?> GetByNameAsync(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name cannot be empty.", nameof(name));
        }

        return await db.Equipments
            .AsNoTracking()
            .Include(equipment => equipment.Description)
            .ThenInclude(description => description.Translations)
            .FirstOrDefaultAsync(
                equipment => equipment.Name.ToLower() == name.Trim().ToLower(),
                ct);
    }

    public async Task<IReadOnlyCollection<Equipment>> ListAsync(CancellationToken ct)
    {
        return await db.Equipments
            .AsNoTracking()
            .Include(equipment => equipment.Description)
            .ThenInclude(description => description.Translations)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        return await db.Equipments
            .AsNoTracking()
            .CountAsync(ct);
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
            .AnyAsync(equipment => equipment.Id == id, ct);
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
}