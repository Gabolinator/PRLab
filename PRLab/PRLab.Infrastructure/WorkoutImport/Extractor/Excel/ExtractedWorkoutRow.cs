namespace PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

public sealed record ExtractedWorkoutRow
{
    public int RowNumber { get; init; }

    public IReadOnlyList<ExtractedWorkoutCell> Cells { get; init; } = [];

    public string RawText { get; init; } = string.Empty;
}