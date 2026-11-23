using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public interface IWorkoutImportExtractorCoordinator
{
    Task<IExtractedWorkoutSource> ExtractAsync(
        string? filePath,
        CancellationToken ct = default);
}