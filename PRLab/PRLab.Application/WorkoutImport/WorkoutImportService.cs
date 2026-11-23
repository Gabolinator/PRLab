using PRLab.Application.Models.ImportDraft;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Application.WorkoutImport;

public sealed class WorkoutImportService(
    IWorkoutImportExtractorCoordinator extractor,
    IWorkoutImportInterpreterCoordinator interpreter,
    IWorkoutImportContextFactory contextFactory,
    IAppLogger logger)
    : IWorkoutImportService
{
    public async Task<WorkoutImportResult> ExtractAndInterpretAsync(
        string filePath,
        string? profile,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var extractedSource = await extractor.ExtractAsync(
            filePath,
            ct);

        logger.Log(
            nameof(WorkoutImportService),
            "Workout source extracted successfully.");

        var context = await contextFactory.CreateAsync(
            profile,
            ct);

        var draft = await interpreter.InterpretAsync(
            extractedSource,
            context,
            ct);

        logger.Log(
            nameof(WorkoutImportService),
            "Workout source interpreted successfully.");

        return new WorkoutImportResult
        {
            ExtractedSource = extractedSource,
            Draft = draft
        };
    }
}