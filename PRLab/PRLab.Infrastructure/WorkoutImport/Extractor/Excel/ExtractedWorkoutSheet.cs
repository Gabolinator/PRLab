namespace PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

public sealed record ExtractedWorkoutSheet
{
    public required string Name { get; init; }

    public int Sequence { get; init; }

    public required string RawText { get; init; }

    public IReadOnlyList<ExtractedWorkoutRow> Rows { get; init; } = [];

    public string? SourceReference { get; init; }
}