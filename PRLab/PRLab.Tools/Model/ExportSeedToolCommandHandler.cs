using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Export;
using PRLab.Domain;
using PRLab.Domain.Utilities.Interface;
using PRLab.Tools.Config;
using PRLab.Tools.Model;

namespace PRLab.Tools;

public sealed class ExportSeedToolCommandHandler(
    IServiceProvider services,
    IAppLogger logger,
    ToolCommandUsageLogger usageLogger)
{
    public async Task HandleExportSeedAsync(
        PRToolCommandInputData data,
        PRToolConfig config)
    {
        var exportOrchestrator = services.GetRequiredService<ISeedDataExportOrchestrator>();
        var supportedTargets = exportOrchestrator.GetSupportedTargets();

        logger.Log("Handling Export Data Command...");

        if (!data.IsValidForExport())
        {
            logger.LogWarning(
                $"Invalid Command or Missing Export Target : Command: {(string.IsNullOrWhiteSpace(data.Command) ? "null" : data.Command)} / Target : {(string.IsNullOrWhiteSpace(data.Target) ? "null" : data.Target)}");

            usageLogger.PrintExportUsage(supportedTargets);
            return;
        }

        if (data.TargetIsAll)
        {
            await ExportAllSeedDataAsync(exportOrchestrator);
            return;
        }

        if (!SeedTargets.TryGetTarget(
                data.Target,
                config,
                out var normalizedTarget,
                out _)
            || !supportedTargets.Contains(normalizedTarget))
        {
            logger.LogWarning($"Target: {data.Target} - Invalid");
            usageLogger.PrintExportUsage(supportedTargets);
            return;
        }

        var resolvedFilePath = data.FilePath
                               ?? exportOrchestrator.GetDefaultFilePath(normalizedTarget);

        if (File.Exists(resolvedFilePath)
            && !ConfirmOverwrite(resolvedFilePath))
        {
            logger.Log($"Export cancelled. File already exists: {resolvedFilePath}", Color.Red);
            return;
        }

        logger.Log($"Exporting seed data for {normalizedTarget}...");

        await exportOrchestrator.ExportAsync(
            normalizedTarget,
            resolvedFilePath);

        logger.Log(
            $"Exported '{normalizedTarget}' seed data to default seed directory '{resolvedFilePath}'",
            Color.Green);
    }

    public async Task AskToExportChangedSeedDataAsync(
        IReadOnlyCollection<SeedResult> changedResults,
        PRToolConfig config)
    {
        if (changedResults.Count == 0)
        {
            return;
        }

        var exportOrchestrator = services.GetRequiredService<ISeedDataExportOrchestrator>();
        var supportedExportTargets = exportOrchestrator.GetSupportedTargets();

        foreach (var result in changedResults)
        {
            if(!result.EntityType.IsBaseType())
            {
                logger.Log($"Dependant entity: {result.EntityType} wont be exported");
                return;
            }

            
            if (!SeedTargets.TryGetTargetAlias(
                    result.EntityType,
                    config,
                    out var exportTarget)
                || !supportedExportTargets.Contains(exportTarget))
            {
                if (result.EntityType.IsBaseType())
                {
                    logger.LogWarning(
                        $"No seed export target is available for changed entity: {result.EntityType}");
                }
                
                else
                {
                    logger.Log($"Dependant entity: {result.EntityType} wont be exported");
                }
                
                continue;
            }

            var changedKeys = string.Join(
                ", ",
                result.Changes.Select(change => $"{change.Type}:{change.Key}"));

            logger.LogWarning(
                $"Seed changed {result.ChangeCount} item(s) for {result.EntityType}: {changedKeys}");

            if (!ConfirmExportChangedSeedFile(result.EntityType, exportTarget))
            {
                logger.Log($"Skipped export for {result.EntityType}");
                continue;
            }

            var resolvedFilePath = exportOrchestrator.GetDefaultFilePath(exportTarget);

            if (File.Exists(resolvedFilePath)
                && !ConfirmOverwrite(resolvedFilePath))
            {
                logger.Log($"Export cancelled. File already exists: {resolvedFilePath}", Color.Red);
                continue;
            }

            await exportOrchestrator.ExportAsync(
                exportTarget,
                resolvedFilePath);

            logger.Log(
                $"Exported '{exportTarget}' seed data to '{resolvedFilePath}'",
                Color.Green);
        }
    }

    private async Task ExportAllSeedDataAsync(ISeedDataExportOrchestrator exportOrchestrator)
    {
        var supportedTargets = exportOrchestrator.GetSupportedTargets()
            .OrderBy(target => target)
            .ToList();

        logger.Log("Exporting all seed data...");

        var exportedTargets = new List<string>();
        var skippedTargets = new List<string>();

        foreach (var target in supportedTargets)
        {
            var resolvedFilePath = exportOrchestrator.GetDefaultFilePath(target);

            if (File.Exists(resolvedFilePath)
                && !ConfirmOverwrite(resolvedFilePath))
            {
                logger.Log($"Skipped export for '{target}'. File already exists: {resolvedFilePath}", Color.Yellow);
                skippedTargets.Add(target);
                continue;
            }

            await exportOrchestrator.ExportAsync(
                target,
                resolvedFilePath);

            logger.Log(
                $"Exported '{target}' seed data to '{resolvedFilePath}'",
                Color.Green);

            exportedTargets.Add(target);
        }

        logger.Log(
            $"Export all seed data completed. Exported: {exportedTargets.Count}. Skipped: {skippedTargets.Count}.",
            Color.Green);
    }
    
    private bool ConfirmOverwrite(string filePath)
    {
        logger.LogWarning("File already exists: ");
        logger.Log($" -> {filePath}", Color.Yellow);
        logger.Log("Overwrite? Type 'y' to overwrite or anything else to cancel:", Color.Yellow);

        var input = Console.ReadLine();

        return string.Equals(
            input?.Trim(),
            "y",
            StringComparison.OrdinalIgnoreCase);
    }

    private bool ConfirmExportChangedSeedFile(
        DomainEnum.EntityType entityType,
        string exportTarget)
    {
        logger.Log(
            $"Regenerate exported seed JSON for {entityType} ({exportTarget})?",
            Color.Yellow);

        logger.Log("Type 'y' to export or anything else to skip:", Color.Yellow);

        var input = Console.ReadLine();

        return string.Equals(
            input?.Trim(),
            "y",
            StringComparison.OrdinalIgnoreCase);
    }
}