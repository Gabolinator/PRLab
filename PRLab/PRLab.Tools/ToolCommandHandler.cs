using PRLab.Domain.Utilities.Interface;
using PRLab.Tools.Commands;
using PRLab.Tools.Model;

namespace PRLab.Tools;

public sealed class ToolCommandHandler(
    IAppLogger logger,
    ToolCommandUsageLogger usageLogger,
    SeedToolCommandHandler seedToolCommandHandler,
    ExportSeedToolCommandHandler exportSeedToolCommandHandler,
    ImportWorkoutCommandHandler importWorkoutCommandHandler)
{
    public async Task HandleAsync(PRToolCommandInputData inputData, PRToolConfig config)
    {
        if (!inputData.IsValid())
        {
            logger.LogWarning("Input is Invalid");
            usageLogger.PrintUsage();
            return;
        }

        if (!PRToolCommandHelper.TryGetCommand(inputData.Command, config, out var command))
        {
            logger.LogWarning($"Unknown command: {inputData.Command}");
            usageLogger.PrintUsage();
            return;
        }

        switch (command)
        {
            case ToolCommands.Seed:
                await seedToolCommandHandler.HandleSeedAsync(inputData, config);
                return;

            case ToolCommands.Export:
                await exportSeedToolCommandHandler.HandleExportSeedAsync(inputData, config);
                return;
            case ToolCommands.ImportWorkout:
                await importWorkoutCommandHandler.HandleAsync(
                    inputData);
                return;

            default:
                logger.Log($"Unknown command: {command}");
                usageLogger.PrintUsage();
                return;
        }
    }
}