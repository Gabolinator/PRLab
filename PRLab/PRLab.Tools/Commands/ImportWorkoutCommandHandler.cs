using PRLab.Application.WorkoutImport;
using PRLab.Domain.Utilities.Interface;
using PRLab.Tools.Model;

namespace PRLab.Tools.Commands;

public sealed class ImportWorkoutCommandHandler(
    IWorkoutImportService importService,
    IAppLogger logger)
{
    public async Task HandleAsync(
        PRToolCommandInputData inputData,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(inputData);

        if (!inputData.IsValidForWorkoutImport())
        {
            logger.LogWarning(
                "Workout import requires a valid file path.");

            return;
        }

        var filePath = inputData.FilePath!;

        await importService.ExtractAndInterpretAsync(filePath, inputData.Profile, ct);
    }
}
