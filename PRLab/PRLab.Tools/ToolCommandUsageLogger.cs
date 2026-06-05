using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Export;
using PRLab.Domain;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Seeding.Export;

namespace PRLab.Tools;

public sealed class ToolCommandUsageLogger(
    IServiceProvider services,
    IAppLogger logger)
{
    public void PrintUsage()
    {
        var exportOrchestrator = services.GetService<ISeedDataExportOrchestrator>();

        if (exportOrchestrator is not null)
        {
            PrintExportUsage(exportOrchestrator.GetSupportedTargets());
        }

        var dataSeeder = services.GetService<IDataSeeder>();

        if (dataSeeder is not null)
        {
            PrintSeedUsage(dataSeeder.EntitySeederTypes.Select(SeedReferences.GetEntityAlias).ToList());
        }
    }

    public void PrintSeedUsage(IReadOnlyList<string> entities)
    {
        logger.Log("/**** Seed Usage ****/", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- seed all", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- seed equipment", Color.Red);

        PrintSeedableTargets(entities);
    }

    public void PrintExportUsage(IReadOnlySet<string> supportedTargets)
    {
        logger.Log("/**** Export Usage ****/", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- export-seed all", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- export-seed equipment", Color.Red);
        logger.Log("  dotnet run --project PRLab.Tools -- export-seed equipment <output-path>", Color.Red);

        PrintExportTargets(supportedTargets);
    }

    private void PrintSeedableTargets(IReadOnlyList<string> entities)
    {
        logger.Log(" -> Available seed entities:", Color.Red);
        logger.Log(
            "    all, " + string.Join(
                ", ",
                entities
                    .Select(entity => entity.ToLower())
                    .OrderBy(target => target)),
            Color.Red);
    }

    private void PrintExportTargets(IReadOnlySet<string> supportedTargets)
    {
        logger.Log(" -> Available export targets:", Color.Red);
        logger.Log(
            "    all, " + string.Join(
                ", ",
                supportedTargets.OrderBy(target => target)),
            Color.Red);
    }
}