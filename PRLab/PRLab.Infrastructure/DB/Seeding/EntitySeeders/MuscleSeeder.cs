using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Muscle;
using PRLab.Application.Interface.UserService;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Seeding.EntitySeeders;

public sealed class MuscleSeeder(
    PRLabPgDBContext db,
    ISystemUserProvider userService,
    IMuscleSeedFactory seedFactory,
    IAppLogger logger) : EntitySeederBase(db, logger)
{
    public override string Name => "DevelopmentMuscleSeed";

    public override string Version => "1.0.0";

    public override EntityType EntityType => EntityType.Muscle;

    public override User SeedUser => userService.GetSystemAdminUser("Seed");
    
    protected override async Task<IReadOnlyList<SeedChange>> SeedEntityAsync(SeedExecutionOptions options, CancellationToken ct)
    {
        var muscleSeedItems = seedFactory.CreateInitialData(options);

        var changes = new List<SeedChange>();

        foreach (var muscleSeedItem in muscleSeedItems)
        {
            var result = await ApplyMuscleSeedItem(muscleSeedItem, ct);

            if (result.change is not null)
            {
                changes.Add(result.change);
            }
        }

        return changes;
    }

    private async Task<(Muscle? entity, SeedChange? change)> ApplyMuscleSeedItem(
        SeedItem<Muscle> muscleSeedItem,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(muscleSeedItem);

        if (muscleSeedItem.Action == SeedAction.Ignore)
        {
            return (null, null);
        }

        var seedMuscle = muscleSeedItem.Entity;

        var existingMuscle = await db.Muscles
            .ForFullWrite()
            .FirstOrDefaultAsync(
                muscle => muscle.NameKey == seedMuscle.NameKey,
                ct);

        if (existingMuscle is null)
        {
            /*
             * seedMuscle already contains its MuscleFunctionAssignment rows,
             * so EF will insert them with the new muscle.
             */
            await db.Muscles.AddAsync(
                seedMuscle,
                ct);

            logger.Log(
                $"Seeded - {EntityType} : {seedMuscle.NameKey}");

            return (
                seedMuscle,
                new SeedChange(
                    seedMuscle.NameKey,
                    SeedChangeType.Created));
        }

        if (muscleSeedItem.Action == SeedAction.CreateIfMissing)
        {
            return (existingMuscle, null);
        }

        logger.Log(
            $"Seeder Updating - {EntityType} : {seedMuscle.NameKey}");

        var muscleChanged = existingMuscle.Update(
            MuscleUpdate.FromMuscle(
                seedMuscle,
                language: null,
                SeedUser));

        var requestedFunctions = seedMuscle.Functions
            .Select(functionAssignment =>
                new MuscleFunctionDefinition(
                    functionAssignment.Function,
                    functionAssignment.Role))
            .ToList();

        var functionsChanged = existingMuscle.ReplaceFunctions(
            requestedFunctions,
            SeedUser);

        var hasChanged =
            muscleChanged ||
            functionsChanged;

        return hasChanged
            ? (
                existingMuscle,
                new SeedChange(
                    seedMuscle.NameKey,
                    SeedChangeType.Updated))
            : (existingMuscle, null);
    }
}