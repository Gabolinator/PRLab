namespace PRLab.Application.WorkoutImport;

public interface IWorkoutImportService
{
    Task<WorkoutImportResult> ExtractAndInterpretAsync(
        string filePath,
        string? profile,
        CancellationToken ct = default);
}