using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class EquipmentRepository(
    PRLabPgDBContext db) : IEquipmentRepository
{
    public async Task<Equipment?> GetByIdAsync(
        EquipmentId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseEquipmentReadQuery()
            .FirstOrDefaultAsync(
                equipment => equipment.Id == id,
                ct);
    }

    public async Task<Equipment?> GetTrackedByIdAsync(
        EquipmentId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseEquipmentWriteQuery()
            .FirstOrDefaultAsync(
                equipment => equipment.Id == id,
                ct);
    }

    public async Task<Equipment?> GetByNameAsync(
        string name,
        CancellationToken ct)
    {
        ValidateName(name);

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseEquipmentReadQuery()
            .FirstOrDefaultAsync(
                equipment => equipment.NameKey == nameKey,
                ct);
    }

    public async Task<IReadOnlyCollection<Equipment>> ListAsync(
        CancellationToken ct)
    {
        return await BaseEquipmentReadQuery()
            .OrderBy(equipment => equipment.Name)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        CancellationToken ct)
    {
        return await BaseEquipmentLookupQuery()
            .CountAsync(ct);
    }

    public async Task<Equipment> CreateAsync(
        Equipment equipment,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        await db.Equipments.AddAsync(equipment, ct);
        await db.SaveChangesAsync(ct);

        return equipment;
    }

    public async Task<Equipment> UpdateAsync(
        Equipment equipment,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(equipment);
        ValidateId(equipment.Id);

        if (db.Entry(equipment).State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                "Equipment must be loaded with GetByIdForUpdateAsync " +
                "before it can be updated.");
        }

        await db.SaveChangesAsync(ct);

        return equipment;
    }

    public async Task<bool> ExistsAsync(
        EquipmentId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseEquipmentLookupQuery()
            .AnyAsync(
                equipment => equipment.Id == id,
                ct);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        EquipmentId? excludedEquipmentId,
        CancellationToken ct)
    {
        ValidateName(name);

        if (excludedEquipmentId.HasValue)
        {
            ValidateId(excludedEquipmentId.Value);
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseEquipmentLookupQuery()
            .AnyAsync(
                equipment =>
                    equipment.NameKey == nameKey &&
                    (!excludedEquipmentId.HasValue ||
                     equipment.Id != excludedEquipmentId.Value),
                ct);
    }

    private IQueryable<Equipment> BaseEquipmentReadQuery()
    {
        return db.Equipments
            .ForFullRead();
    }

    private IQueryable<Equipment> BaseEquipmentWriteQuery()
    {
        return db.Equipments
            .ForFullWrite();
    }

    private IQueryable<Equipment> BaseEquipmentLookupQuery()
    {
        return db.Equipments
            .ActiveOnly()
            .AsNoTracking();
    }

    private static void ValidateId(EquipmentId id)
        => DomainGuard.ValidRequiredId(id, nameof(id));
    

    private static void ValidateName(string name)
        => DomainGuard.NotEmptyName(name, nameof(name));
}