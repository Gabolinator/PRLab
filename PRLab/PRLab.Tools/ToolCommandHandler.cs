using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Export;
using PRLab.Domain;
using PRLab.Domain.Utilities.Interface;
using PRLab.Tools.Model;

namespace PRLab.Tools;

public sealed class ToolCommandHandler(
    IServiceProvider services,
    IAppLogger logger)
{
    public async Task HandleAsync(PRToolCommandInputData inputData, PRToolConfig config)
    {
        if (!inputData.IsValid())
        {
            logger.LogWarning($"Input is Invalid");
            PrintUsage();
            return;
        }

        if (!PRToolCommandHelper.TryGetCommand(inputData.Command, config ,out var command))
        {
            logger.LogWarning($"Unknown command: {inputData.Command}");
            PrintUsage();
            return;
        }

        switch (command)
        {
            case ToolCommands.Seed:
                await HandleSeedAsync(inputData, config);
                return;

            case ToolCommands.Export:
                await HandleExportSeedAsync(inputData, config);
                return;

            default:
                logger.Log($"Unknown command: {command}");
                PrintUsage();
                return;
        }
    }

    private async Task HandleSeedAsync(
        PRToolCommandInputData data,
        PRToolConfig config)
    {
        var dataSeeder = services.GetRequiredService<IDataSeeder>();
        var supportedTargets = dataSeeder.EntitySeederTypes;
        if (!data.IsValidForSeed())
        {
            logger.LogWarning($"Invalid Command or Missing Seed Target : Command: {(string.IsNullOrWhiteSpace(data.Command)? "null" :  data.Command)} / Target : {(string.IsNullOrWhiteSpace(data.Target)? "null" :  data.Target)}");
            PrintSeedUsage(supportedTargets);
            return;
        }

        var seedEntities = new List<DomainEnum.EntityType>();
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
            PrintSeedUsage(supportedTargets);
            return;
        }
        else
        {
            seedEntities = [entity];
        }
        
        var seedEntityNames = string.Join(", ", seedEntities);

        logger.Log($"Handling seeding data for: {(data.TargetIsAll ? "all" : string.Empty)} [{seedEntityNames}]...");

        try
        {
            var results = await dataSeeder.SeedAsync(seedEntities);
            var changes = results.Where(result => result.Changed).ToList();
            
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
                await AskToExportChangedSeedDataAsync(
                    changes,
                    config);
            }

            else
            {
                logger.Log("No Changes Detected in seed", Color.Yellow);
            }
        }
        catch (Exception e)
        {
            logger.LogError($"Seeding data for [{seedEntityNames}] failed : {e.Message}");
            throw;
        }
    }

    private async Task HandleExportSeedAsync(
        PRToolCommandInputData data,
        PRToolConfig config)
    {
        var exportOrchestrator = services.GetRequiredService<ISeedDataExportOrchestrator>();
        var supportedTargets = exportOrchestrator.GetSupportedTargets();

        logger.Log("Handling Export Data Command...");
        
        if (!data.IsValidForExport())
        {
            logger.LogWarning($"Invalid Command or Missing Export Target : Command: {(string.IsNullOrWhiteSpace(data.Command)? "null" :  data.Command)} / Target : {(string.IsNullOrWhiteSpace(data.Target)? "null" :  data.Target)}");
            PrintExportUsage(supportedTargets);
            return;
        }

        if (data.TargetIsAll)
        {
            if (!ConfirmOverwriteAll(exportOrchestrator))
            {
                logger.Log("Export cancelled.", Color.Red);
                return;
            }

            logger.Log("Exporting all seed data...");

            await exportOrchestrator.ExportAllAsync();

            logger.Log("Exported all seed data.", Color.Green);
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
            PrintExportUsage(supportedTargets);
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

        logger.Log($"Exported '{normalizedTarget}' seed data to default seed directory '{resolvedFilePath}'",  Color.Green);
    }

    private bool ConfirmOverwriteAll(ISeedDataExportOrchestrator exportOrchestrator)
    {
        var existingFiles = exportOrchestrator.GetSupportedTargets()
            .Select(target => exportOrchestrator.GetDefaultFilePath(target))
            .Where(File.Exists)
            .OrderBy(path => path)
            .ToList();

        if (existingFiles.Count == 0)
        {
            return true;
        }

        logger.LogWarning("The following seed files already exist:");

        foreach (var filePath in existingFiles)
        {
            logger.Log($" -> {filePath}", Color.Yellow);
        }

        logger.Log("Overwrite all existing files? Type 'y' to overwrite or anything else to cancel:", Color.Yellow);

        var input = Console.ReadLine();

        return string.Equals(
            input?.Trim(),
            "y",
            StringComparison.OrdinalIgnoreCase);
    }
    
    private bool ConfirmOverwrite(string filePath)
    {
        logger.LogWarning($"File already exists: ");
        logger.Log($" -> {filePath}", Color.Yellow);
        logger.Log("Overwrite? Type 'y' to overwrite or anything else to cancel:", Color.Yellow);

        var input = Console.ReadLine();

        return string.Equals(
            input?.Trim(),
            "y",
            StringComparison.OrdinalIgnoreCase);
    }
    
    private async Task AskToExportChangedSeedDataAsync(
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
            if (!SeedTargets.TryGetTargetAlias(
                    result.EntityType,
                    config,
                    out var exportTarget)
                || !supportedExportTargets.Contains(exportTarget))
            {
                logger.LogWarning($"No seed export target is available for changed entity: {result.EntityType}");
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

    private bool ConfirmExportChangedSeedFile(
        DomainEnum.EntityType entityType,
        string exportTarget)
    {
        logger.Log(
            $"Regenerate exported seed JSON for {entityType} ({exportTarget})?", Color.Yellow);
        logger.Log("Type 'y' to export or anything else to skip:", Color.Yellow);
       
        var input = Console.ReadLine();

        return string.Equals(
            input?.Trim(),
            "y",
            StringComparison.OrdinalIgnoreCase);
    }
    
    private void PrintUsage()
    {
        var exportOrchestrator = services.GetService<ISeedDataExportOrchestrator>();

        if (exportOrchestrator is not null)
        {
            PrintExportUsage(exportOrchestrator.GetSupportedTargets());
        }

        var dataSeeder = services.GetService<IDataSeeder>();

        if (dataSeeder is not null)
        {
            PrintSeedUsage(dataSeeder.EntitySeederTypes);
        }
    }

    private void PrintSeedUsage(IReadOnlySet<DomainEnum.EntityType> entities)
    {
        logger.Log("/**** Seed Usage ****/", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- seed all", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- seed equipment", Color.Red);
        
        PrintSeedableTargets(entities);
    }
    
    private void PrintSeedableTargets(IReadOnlySet<DomainEnum.EntityType> entities)
    {
        logger.Log(" -> Available seed entities:", Color.Red);
        logger.Log("    all, " + string.Join(", ", entities.Select(entity=> entity.ToString().ToLower()).OrderBy(target => target)), Color.Red);
    }

    private void PrintExportUsage(IReadOnlySet<string> supportedTargets)
    {
        logger.Log("/**** Export Usage ****/", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- export-seed all", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- export-seed equipment", Color.Red);
        logger.Log( " dotnet run --project PRLab.Tools -- export-seed equipment <output-path>", Color.Red);

        PrintExportTargets(supportedTargets);
    }

    private void PrintExportTargets(IReadOnlySet<string> supportedTargets)
    {
        logger.Log(" -> Available export targets:", Color.Red);
        logger.Log("    all, " + string.Join(", ", supportedTargets.OrderBy(target => target)), Color.Red);
    }
}