using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories;

public class SeedHistoryRepository(PRLabPgDBContext db) : ISeedHistoryRepository
{
    public async Task<IReadOnlyList<SeedHistory>> ListAsync(CancellationToken ct)
    {
        return await db.SeedHistory.ToListAsync(ct);
    }
}