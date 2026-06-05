using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public abstract class EntitySeederBase(PRLabPgDBContext db, IAppLogger logger) : IEntitySeeder
{
    public abstract DomainEnum.EntityType EntityType { get; }
    public int Order => SeedPolicy.GetSeedOrder(EntityType);
    public abstract string Name { get; }
    public abstract string Version { get; }
    public abstract User SeedUser { get; }

    public async Task<SeedResult> SeedAsync(CancellationToken ct = default)
    {
        logger.Log(nameof(EntitySeederBase), $"Trying to Seed {EntityType} - {Name} {Version}");

        var alreadySeeded = await db.SeedHistory
            .AnyAsync(
                seedHistory => seedHistory.Name == Name
                               && seedHistory.Version == Version,
                ct);

        if (alreadySeeded)
        {
            logger.LogWarning(
                nameof(EntitySeederBase),
                $"Already Seeded - {Name} {Version} - Skipping Seed. Bump Version to rerun this seeder.");
            return SeedResult.Skipped(EntityType, Name, Version);
        }

        try
        {
            var changes = await SeedEntityAsync(ct);

            await db.SeedHistory.AddAsync(
                SeedHistory.New(Name, Version, DateTimeOffset.UtcNow),
                ct);

            await db.SaveChangesAsync(ct);

            return SeedResult.FromChanges(EntityType, Name, Version, changes);
              
        }
        catch (Exception e)
        {
            logger.LogError(nameof(EntitySeederBase), $"Seeding {Name} {Version} failed : {e.GetBaseException().Message}");
            throw;
        }
    }

    protected abstract Task<IReadOnlyList<SeedChange>> SeedEntityAsync(CancellationToken ct);
}