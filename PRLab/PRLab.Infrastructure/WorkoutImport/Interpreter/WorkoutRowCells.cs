using System.Globalization;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutRowCells
{
    private WorkoutRowCells(DateTime date, int dateColumnIndex, IReadOnlyList<WorkoutRowCell> cells)
    {
        Date = date;
        DateColumnIndex = dateColumnIndex;
        Cells = cells;
    }

    public DateTime Date { get; }

    public int DateColumnIndex { get; }

    public int ColumnOffset => DateColumnIndex;
    
    public int LastColumnIndex => Cells.Count - 1;

    public IReadOnlyList<WorkoutRowCell> Cells { get; }

    public static WorkoutRowCells? CreateForDatedRow(
        ExtractedWorkoutRow row, ILookup<int, WorkoutImportProperty> expectedProperties)
    {
        if (FindDateCell(row) is not { } dateMatch)
        {
            return null;
        }

        var (sourceDateCell, date) = dateMatch;
        var dateColumnIndex = sourceDateCell.ColumnIndex;
        var cells = BuildCells(row, expectedProperties, dateColumnIndex);
        var dateCell = cells[dateColumnIndex];
        var dateSpan = new TextSpan(0, dateCell.Text.Length);
        dateCell.Consume(dateSpan, ConsumptionKind.Date);

        return new WorkoutRowCells(date, dateColumnIndex, cells);
    }

    public WorkoutRowCell? GetExpectedCell(WorkoutImportProperty property) =>
        Cells.FirstOrDefault(cell => cell.ExpectedProperties.Contains(property));

    public IEnumerable<UnconsumedCellText> GetUnconsumedText() =>
        Cells.SelectMany(cell => cell.GetUnconsumedText());

    private static (ExtractedWorkoutCell Cell, DateTime Date)? FindDateCell(ExtractedWorkoutRow row)
    {
        var cellsInColumnOrder = row.Cells.OrderBy(cell => cell.ColumnIndex);
        foreach (var cell in cellsInColumnOrder)
        {
            if (TryReadDate(cell.Value, out var date))
            {
                return (cell, date);
            }
        }

        return null;
    }

    private static bool TryReadDate(string? text, out DateTime date)
    {
        var trimmedText = text?.Trim();
        var culture = CultureInfo.InvariantCulture;
        var parsed = DateTime.TryParseExact(trimmedText, "d.M.yyyy", culture, DateTimeStyles.None, out date)
                     || DateTime.TryParse(trimmedText, culture, DateTimeStyles.NoCurrentDateDefault, out date)
                     && date.Date != DateTime.MinValue;
        date = date.Date;
        return parsed;
    }

    private static WorkoutRowCell[] BuildCells(
        ExtractedWorkoutRow row, ILookup<int, WorkoutImportProperty> expectedProperties, int columnOffset)
    {
        var sourceCellsByColumn = row.Cells.ToDictionary(cell => cell.ColumnIndex);
        var lastSourceColumn = sourceCellsByColumn.Keys.Max();
        var lastExpectedColumn = expectedProperties.Max(group => group.Key) + columnOffset;
        var columnCount = Math.Max(lastSourceColumn, lastExpectedColumn) + 1;

        var columns = Enumerable.Range(0, columnCount);
        return columns.Select(columnIndex =>
        {
            var sourceCell = sourceCellsByColumn.GetValueOrDefault(columnIndex);
            var properties = expectedProperties[columnIndex - columnOffset].ToArray();
            return new WorkoutRowCell(columnIndex, sourceCell, properties);
        }).ToArray();
    }
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
        var consumptionInTextOrder = consumption.OrderBy(entry => entry.Span.Start);
        foreach (var entry in consumptionInTextOrder)
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

internal enum ConsumptionKind
{
    Date,
    Order,
    Exercise,
    Equipment,
    Attribute,
    Prescription,
    WorkWindow,
    Ignored
}

internal sealed record CellConsumption(TextSpan Span, ConsumptionKind Kind);

internal sealed record UnconsumedCellText(WorkoutRowCell Cell, TextSpan Span)
{
    public string Text => Cell.Text.Substring(Span.Start, Span.Length);
}
