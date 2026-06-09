using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories.Entity;

public interface IEquipmentRepository
{
    
    Task<Equipment?> GetByIdAsync(EquipmentId id, CancellationToken ct);

    Task<Equipment?> GetByNameAsync(string name, CancellationToken ct);

    Task<IReadOnlyCollection<Equipment>> ListAsync(CancellationToken ct);
    
    Task<int> CountAsync(CancellationToken ct);

    Task<Equipment> CreateAsync(Equipment equipment, CancellationToken ct);

    Task<Equipment> UpdateAsync(Equipment equipment, CancellationToken ct);

    Task<bool> ExistsAsync(EquipmentId id, CancellationToken ct);

    Task<bool> NameExistsAsync(string name, EquipmentId? excludedEquipmentId, CancellationToken ct);

}