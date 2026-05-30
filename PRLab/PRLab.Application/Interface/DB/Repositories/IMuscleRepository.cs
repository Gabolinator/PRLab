using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories;

public interface IMuscleRepository
{
    Task<IReadOnlyList<Muscle>> ListAsync(CancellationToken ct);

    Task<Muscle?> GetByIdAsync(MuscleId id, CancellationToken ct);

    Task<Muscle> CreateAsync(Muscle muscle, CancellationToken ct);

    Task<Muscle> UpdateAsync(Muscle muscle, CancellationToken ct);

    Task<bool> ExistsAsync(MuscleId id, CancellationToken ct);

    Task<bool> AllExistAsync(
        IReadOnlyCollection<MuscleId> ids,
        CancellationToken ct);

    Task<Muscle> UpdateAntagonistsAsync(
        MuscleId id,
        IReadOnlyCollection<MuscleId> antagonistIds,
        CancellationToken ct);
    
    Task<bool> NameExistsAsync(
        string name,
        MuscleId? excludedMuscleId,
        CancellationToken ct);
}