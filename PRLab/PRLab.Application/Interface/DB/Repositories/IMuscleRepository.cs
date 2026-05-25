using PRLab.Application.Results.APIResults;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories;

public interface IMuscleRepository
{
    Task<Muscle?> GetByIdAsync(MuscleId id, CancellationToken ct);

    Task<Muscle?> GetByNameAsync(string name, CancellationToken ct);

    Task<IReadOnlyCollection<Muscle>> ListAsync(CancellationToken ct);

    Task<IReadOnlyCollection<Muscle>> ListByBodySectionAsync(
        DomainEnum.BodySection bodySection,
        CancellationToken ct);

    Task<Muscle> CreateAsync(Muscle muscle, CancellationToken ct);

    Task<Muscle> UpdateAsync(Muscle muscle, CancellationToken ct);

    Task<bool> ExistsAsync(MuscleId id, CancellationToken ct);

    Task<bool> NameExistsAsync(string name, CancellationToken ct);
}