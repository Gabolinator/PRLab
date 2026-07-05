using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Utilities.Interface;
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
            var options = ResolveSeedExecutionOptions(data);

            usageLogger.LogSeedOptions(options);

            if (!ConfirmSeedExecution(options))
            {
                logger.Log("Seed cancelled.", Color.Yellow);
                return;
            }
            
            var results = await dataSeeder.SeedAsync(
                seedEntities,
                options);    var changes = results
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
    
    private static bool RequiresDangerousSeedConfirmation(
        SeedExecutionOptions options)
    {
        return options.IgnoreSeedHistory
               || options.ActionOverride.HasValue
               || options.IgnoreTopLevelIds
               || options.IgnoreReferenceIds;
    }
    
    private bool ConfirmSeedExecution(
        SeedExecutionOptions options)
    {
        if (!RequiresDangerousSeedConfirmation(options))
        {
            return true;
        }

        logger.LogWarning(
            "Are you sure you want to proceed with seed with those options? " +
            "There could be destructive seeding that cannot be reverted.");

        logger.LogWarning(
            "Type 'y' to confirm. Anything else will cancel.");

        var firstConfirmation = Console.ReadLine();

        if (!string.Equals(firstConfirmation, "y", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        logger.LogWarning(
            "Type 'y' a second time to really confirm. Anything else will cancel.");

        var secondConfirmation = Console.ReadLine();

        return string.Equals(secondConfirmation, "y", StringComparison.OrdinalIgnoreCase);
    }
    
    private static SeedExecutionOptions ResolveSeedExecutionOptions(
        PRToolCommandInputData data)
    {
        if (data.HasOption("--full-reseed", "--reseed"))
        {
            return SeedExecutionOptions.FullReseed;
        }

        return SeedExecutionOptions.Default;
    }
}