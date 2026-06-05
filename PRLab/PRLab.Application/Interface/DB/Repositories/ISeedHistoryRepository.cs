using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Application.Interface.DB.Repositories;

public interface ISeedHistoryRepository
{
    Task<IReadOnlyList<SeedHistory>> ListAsync(CancellationToken ct);
}