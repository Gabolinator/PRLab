namespace PRLab.Application.WorkoutImport.Model;

public sealed record ExtractedWorkoutSource
{
    public required WorkoutImportSourceType SourceType { get; init; }

    public string? SourceName { get; init; }

    public required string NormalizedText { get; init; }

    public IReadOnlyList<ExtractedWorkoutSection> Sections { get; init; } = [];

    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();
}