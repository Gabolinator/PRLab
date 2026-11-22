namespace PRLab.Application.WorkoutImport.Model;

public sealed record ExtractedWorkoutSection
{
    public required string Name { get; init; }

    public int Sequence { get; init; }

    public required string RawText { get; init; }

    public IReadOnlyList<ExtractedWorkoutRow> Rows { get; init; } = [];

    public string? SourceReference { get; init; }
}

public sealed record ExtractedWorkoutRow
{
    public int RowNumber { get; init; }

    public IReadOnlyList<ExtractedWorkoutCell> Cells { get; init; } = [];

    public string RawText { get; init; } = string.Empty;
}

public sealed record ExtractedWorkoutCell
{
    public int ColumnIndex { get; init; }

    public string ColumnName { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;

    public string SourceReference { get; init; } = string.Empty;
}