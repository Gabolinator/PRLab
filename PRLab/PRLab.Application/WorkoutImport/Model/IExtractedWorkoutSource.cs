namespace PRLab.Application.WorkoutImport.Model;

public interface IExtractedWorkoutSource
{
    WorkoutImportSourceType SourceType { get; }

    string? SourceName { get; }

    string NormalizedText { get; }
}