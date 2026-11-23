namespace PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

public sealed record ExtractedWorkoutCell
{
    public int ColumnIndex { get; init; }

    public string ColumnName { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;

    public string SourceReference { get; init; } = string.Empty;
}