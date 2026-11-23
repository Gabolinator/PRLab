using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

public sealed record ExtractedExcelWorkoutSource : IExtractedWorkoutSource
{
    public required WorkoutImportSourceType SourceType { get; init; }

    public string? SourceName { get; init; }

    public required string NormalizedText { get; init; }

    public IReadOnlyList<ExtractedWorkoutSheet> Sheets { get; init; } = [];

    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();
}