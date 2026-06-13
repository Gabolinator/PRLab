using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Enum.System;
using PRLab.Infrastructure.DB.Seeding.Export;
using PRLab.Tools.Config;
using PRLab.Tools.Model;

namespace PRLab.Tools;

public sealed class SeedToolCommandHandler(
    IServiceProvider services,
    IAppLogger logger,
    ToolCommandUsageLogger usageLogger,
    ExportSeedToolCommandHandler exportSeedToolCommandHandler)
{
    public async Task HandleSeedAsync(
        PRToolCommandInputData data,
        PRToolConfig config)
    {
        var dataSeeder = services.GetRequiredService<IDataSeeder>();
        var supportedTargets = dataSeeder.BaseEntitySeederTypes;

        if (!data.IsValidForSeed())
        {
            logger.LogWarning(
                $"Invalid Command or Missing Seed Target : Command: {(string.IsNullOrWhiteSpace(data.Command) ? "null" : data.Command)} / Target : {(string.IsNullOrWhiteSpace(data.Target) ? "null" : data.Target)}");

            usageLogger.PrintSeedUsage(supportedTargets.Select(SeedReferences.GetEntityAlias).ToList());
            return;
        }

        var seedEntities = new List<EntityType>();

        if (data.TargetIsAll)
        {
            seedEntities = supportedTargets.ToList();
        }
        else if (!SeedTargets.TryGetTarget(
                     data.Target,
                     config,
                     out var normalizedTarget,
                     out var entity)
                 || !supportedTargets.Contains(entity))
        {
            logger.LogWarning($"Target: {data.Target} - Invalid");
            usageLogger.PrintSeedUsage(supportedTargets.Select(SeedReferences.GetEntityAlias).ToList());
            return;
        }
        else
        {
            
            seedEntities = [entity];
        }
        
        seedEntities = SeedReferences.ExpandSeedDependencies(seedEntities, logger).ToList();
        var seedEntityNames = string.Join(", ", seedEntities);

        logger.Log(
            $"Handling seeding data for: {(data.TargetIsAll ? "all" : string.Empty)} [{seedEntityNames}]...");

        try
        {
            var results = await dataSeeder.SeedAsync(seedEntities);
            var changes = results
                .Where(result => result.Changed)
                .ToList();

            var changesSummary = string.Join(
                " | ",
                changes.Select(result =>
                    $"{result.EntityType}: {result.ChangeCount} change(s)"));

            var countChanges = changes.Sum(result => result.ChangeCount);

            logger.Log(
                $"Seeding data - Completed for entity [{seedEntityNames}] - Changed: {changesSummary} - Total changes {countChanges}",
                Color.Green);

            if (changes.Count > 0)
            {
                await exportSeedToolCommandHandler.AskToExportChangedSeedDataAsync(
                    changes,
                    config);
            }
            else
            {
                logger.Log("No Changes Detected in seed", Color.Yellow);
            }
        }
        catch (Exception exception)
        {
            logger.LogError($"Seeding data for [{seedEntityNames}] failed : {exception.Message}");
            throw;
        }
    }
}