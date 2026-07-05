using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public abstract class EntitySeederBase(
    PRLabPgDBContext db,
    IAppLogger logger) : IEntitySeeder
{
    public abstract EntityType EntityType { get; }

    public int Order => SeedPolicy.GetSeedOrder(EntityType);

    public abstract string Name { get; }

    public abstract string Version { get; }

    public abstract User SeedUser { get; }

    public async Task<SeedResult> SeedAsync(
        SeedExecutionOptions options,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        logger.Log(
            nameof(EntitySeederBase),
            $"Trying to Seed {EntityType} - {Name} {Version}");

        var alreadySeeded = await AlreadySeededAsync(ct);

        if (alreadySeeded && !options.IgnoreSeedHistory)
        {
            logger.LogWarning(
                nameof(EntitySeederBase),
                $"Already Seeded - {Name} {Version} - Skipping Seed. Bump Version to rerun this seeder.");

            return SeedResult.Skipped(
                EntityType,
                Name,
                Version);
        }

        if (alreadySeeded && options.IgnoreSeedHistory)
        {
            logger.Log(
                nameof(EntitySeederBase),
                $"Already Seeded - {Name} {Version} - Seed option is ignoring history. Proceeding to seed.");
        }

        try
        {
            var changes = await SeedEntityAsync(
                options,
                ct);

            if (!options.IgnoreSeedHistory)
            {
                await db.SeedHistory.AddAsync(
                    SeedHistory.New(
                        Name,
                        Version,
                        DateTimeOffset.UtcNow),
                    ct);
            }

            await db.SaveChangesAsync(ct);

            return SeedResult.FromChanges(
                EntityType,
                Name,
                Version,
                changes);
        }
        catch (Exception exception)
        {
            logger.LogError(
                nameof(EntitySeederBase),
                $"Seeding {Name} {Version} failed : {exception.GetBaseException().Message}");

            throw;
        }
    }

    public async Task<bool> AlreadySeededAsync(
        CancellationToken ct = default)
    {
        return await db.SeedHistory
            .AnyAsync(
                seedHistory => seedHistory.Name == Name
                               && seedHistory.Version == Version,
                ct);
    }

    protected abstract Task<IReadOnlyList<SeedChange>> SeedEntityAsync(
        SeedExecutionOptions options,
        CancellationToken ct);
}