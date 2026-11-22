using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public interface IWorkoutSourceExtractor
{
    bool CanHandle(WorkoutImportSource source);

    Task<ExtractedWorkoutSource> ExtractAsync(
        WorkoutImportSource source,
        CancellationToken ct = default);
}