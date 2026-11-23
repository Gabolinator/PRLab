using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public interface IWorkoutSourceExtractor
{
    bool CanHandle(WorkoutImportSource source);

    Task<IExtractedWorkoutSource> ExtractAsync(
        WorkoutImportSource source,
        CancellationToken ct = default);
}