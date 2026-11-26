using PRLab.Application.WorkoutImport.Model;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutRowCells
{
    public WorkoutRowCells(
        ExtractedWorkoutRow row, ILookup<int, WorkoutImportProperty> expectedProperties, int dateColumnIndex)
    {
        DateColumnIndex = dateColumnIndex;
        var sourceCells = row.Cells.ToDictionary(cell => cell.ColumnIndex);
        var lastColumn = Math.Max(sourceCells.Keys.Max(), expectedProperties.Max(group => group.Key) + ColumnOffset);
        Cells = Enumerable.Range(0, lastColumn + 1)
            .Select(index => new WorkoutRowCell(
                index, sourceCells.GetValueOrDefault(index), expectedProperties[index - ColumnOffset].ToArray()))
            .ToArray();
    }

    public int DateColumnIndex { get; }

    public int ColumnOffset => DateColumnIndex;

    public IReadOnlyList<WorkoutRowCell> Cells { get; }

    public WorkoutRowCell? GetExpectedCell(WorkoutImportProperty property) =>
        Cells.FirstOrDefault(cell => cell.ExpectedProperties.Contains(property));

    public IEnumerable<UnconsumedCellText> GetUnconsumedText() =>
        Cells.SelectMany(cell => cell.GetUnconsumedText());
}

internal sealed class WorkoutRowCell(
    int columnIndex, ExtractedWorkoutCell? source, IReadOnlyList<WorkoutImportProperty> expectedProperties)
{
    private readonly List<CellConsumption> consumption = [];

    public int ColumnIndex { get; } = columnIndex;

    public string Text { get; } = source?.Value ?? string.Empty;

    public string? SourceReference { get; } = source?.SourceReference;

    public IReadOnlyList<WorkoutImportProperty> ExpectedProperties { get; } = expectedProperties;

    public IReadOnlyList<CellConsumption> Consumption => consumption;

    public void Consume(TextSpan span, ConsumptionKind kind)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(span.Start);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(span.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(span.Length, Text.Length - span.Start);

        consumption.Add(new CellConsumption(span, kind));
    }

    public IEnumerable<UnconsumedCellText> GetUnconsumedText()
    {
        var start = 0;
        foreach (var entry in consumption.OrderBy(entry => entry.Span.Start))
        {
            if (entry.Span.Start > start && !string.IsNullOrWhiteSpace(Text[start..entry.Span.Start]))
            {
                yield return new UnconsumedCellText(this, new TextSpan(start, entry.Span.Start - start));
            }

            start = Math.Max(start, entry.Span.Start + entry.Span.Length);
        }

        if (!string.IsNullOrWhiteSpace(Text[start..]))
        {
            yield return new UnconsumedCellText(this, new TextSpan(start, Text.Length - start));
        }
    }
}

internal readonly record struct TextSpan(int Start, int Length);

internal enum ConsumptionKind
{
    Date,
    Order,
    Exercise
}

internal sealed record CellConsumption(TextSpan Span, ConsumptionKind Kind);

internal sealed record UnconsumedCellText(WorkoutRowCell Cell, TextSpan Span)
{
    public string Text => Cell.Text.Substring(Span.Start, Span.Length);
}
