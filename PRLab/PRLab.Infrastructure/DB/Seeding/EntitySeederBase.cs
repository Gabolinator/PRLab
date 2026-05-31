using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding;

public abstract class EntitySeederBase(PRLabPgDBContext db) : IEntitySeeder
{
    public abstract int Order { get; }
    public abstract string Name { get; }
    public abstract string Version { get; }
    public abstract User SeedUser { get; }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        //just a test
        var alreadySeeded = await db.SeedHistory
            .AnyAsync(
                seedHistory => seedHistory.Name == Name 
                               && seedHistory.Version == Version ,
                ct);

        if (alreadySeeded)
        {
            return;
        }

        await SeedEntityAsync(ct);

        await db.SeedHistory.AddAsync(
            SeedHistory.New(Name, Version ,DateTimeOffset.UtcNow),
            ct);

        await db.SaveChangesAsync(ct);
    }

    protected abstract Task SeedEntityAsync(CancellationToken ct);
}