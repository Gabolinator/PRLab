using PRLab.Application.Results.APIResults;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories
{
    public interface IDescriptionRepository
    {
        Task<Description?> GetByIdAsync(DescriptionId id, CancellationToken ct);

        Task<Description> CreateAsync(Description description, CancellationToken ct);

        Task<Description> UpdateAsync(Description description, CancellationToken ct);

        Task<Description> GetOrCreateAsync(Description description, CancellationToken ct);

        Task<bool> ExistsAsync(DescriptionId id, CancellationToken ct);
    }
}