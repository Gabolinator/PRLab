using System.Text;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

public sealed class CoachExcelWorkoutImportInterpreter(
    IAppLogger logger)
    : WorkoutImportInterpreter<ExtractedExcelWorkoutSource>
{
    public override Task<IReadOnlyList<WorkoutImportDraft>> InterpretAsync(
        ExtractedExcelWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default)
    {
        var profile = context.Profile;
        var excelProfile = profile.GetRequiredSourceProfile<ExcelWorkoutImportSourceProfile>();
        var columns = ReadColumnMapping(excelProfile);
        var matcher = new WorkoutInterpreterHelper(profile, context.Exercises);
        var rowsByDate = InterpretRowsByDate(source.Sheets.SelectMany(sheet => sheet.Rows), columns, matcher, ct);

        logger.Log(nameof(CoachExcelWorkoutImportInterpreter),
            $"Found {rowsByDate.Count} distinct workout dates.");

        IReadOnlyList<WorkoutImportDraft> drafts = rowsByDate
            .Select(entry => CreateWorkoutDraft(entry.Key, source.SourceName, entry.Value))
            .ToList();

        return Task.FromResult(drafts);
    }

    private static ILookup<int, WorkoutImportProperty> ReadColumnMapping(ExcelWorkoutImportSourceProfile profile)
    {
        (int Column, WorkoutImportProperty Property)[] columns =
        [
            (0, WorkoutImportProperty.Date),
            (profile.GetRequiredCell(WorkoutImportProperty.Exercise), WorkoutImportProperty.Exercise),
            (GetOptionalColumn(profile, WorkoutImportProperty.Order), WorkoutImportProperty.Order),
            (GetOptionalColumn(profile, WorkoutImportProperty.Details), WorkoutImportProperty.Details),
            (GetOptionalColumn(profile, WorkoutImportProperty.Notes), WorkoutImportProperty.Notes)
        ];
        return columns.Where(column => column.Column >= 0).ToLookup(column => column.Column, column => column.Property);
    }

    private static int GetOptionalColumn(ExcelWorkoutImportSourceProfile profile, WorkoutImportProperty property) =>
        profile.TryGetColumn(property, out var index) ? index : -1;

    private SortedDictionary<DateTime, List<WorkoutImportRowDraft>> InterpretRowsByDate(
        IEnumerable<ExtractedWorkoutRow> sourceRows,
        ILookup<int, WorkoutImportProperty> columns,
        WorkoutInterpreterHelper matcher,
        CancellationToken ct)
    {
        var rowsByDate = new SortedDictionary<DateTime, List<WorkoutImportRowDraft>>();

        foreach (var row in sourceRows)
        {
            ct.ThrowIfCancellationRequested();

            if (FindDateCell(row) is not { } dateMatch)
            {
                continue;
            }

            var (dateCell, date) = dateMatch;
            var rowCells = new WorkoutRowCells(row, columns, dateCell.ColumnIndex);
            rowCells.Cells[dateCell.ColumnIndex].Consume(new TextSpan(0, dateCell.Value.Length), ConsumptionKind.Date);

            if (!rowsByDate.TryGetValue(date, out var rows))
            {
                rows = [];
                rowsByDate.Add(date, rows);
            }

            var rowDraft = InterpretRow(rowCells, matcher);
            if (rowDraft is null)
            {
                continue;
            }

            rows.Add(rowDraft);
            if (logger.Enabled)
            {
                logger.Log(nameof(CoachExcelWorkoutImportInterpreter), FormatRowLog(date, rowDraft));
            }
        }

        return rowsByDate;
    }

    private static (ExtractedWorkoutCell Cell, DateTime Date)? FindDateCell(ExtractedWorkoutRow row)
    {
        foreach (var cell in row.Cells.OrderBy(cell => cell.ColumnIndex))
        {
            if (WorkoutInterpreterHelper.TryReadDate(cell.Value, out var date))
            {
                return (cell, date);
            }
        }

        return null;
    }

    private static WorkoutImportRowDraft? InterpretRow(WorkoutRowCells row, WorkoutInterpreterHelper matcher)
    {
        var order = ReadOrder(row.GetExpectedCell(WorkoutImportProperty.Order));
        var exercise = FindRowExercise(row, matcher);
        if (exercise is null)
        {
            return null;
        }

        var input = exercise.Value.Input;
        return new WorkoutImportRowDraft
        {
            SourceText = input.Text,
            SourceReference = input.Cell.SourceReference,
            Order = order,
            Details = GetCellText(row, WorkoutImportProperty.Details),
            Notes = GetCellText(row, WorkoutImportProperty.Notes),
            ExerciseParts = exercise.Value.Parts
        };
    }

    private static int? ReadOrder(WorkoutRowCell? cell)
    {
        if (cell is null || cell.Consumption.Count > 0 || !int.TryParse(cell.Text, out var order))
        {
            return null;
        }

        cell.Consume(new TextSpan(0, cell.Text.Length), ConsumptionKind.Order);
        return order;
    }

    private static (UnconsumedCellText Input, IReadOnlyList<WorkoutImportExercisePart> Parts)? FindRowExercise(
        WorkoutRowCells row, WorkoutInterpreterHelper matcher)
    {
        var expectedCell = row.GetExpectedCell(WorkoutImportProperty.Exercise);
        (UnconsumedCellText Input, IReadOnlyList<WorkoutImportExercisePart> Parts)? unresolved = null;
        foreach (var input in row.GetUnconsumedText().OrderByDescending(input => input.Cell == expectedCell))
        {
            if (matcher.IsIgnored(input.Text))
            {
                continue;
            }

            var results = matcher.InterpretExerciseCell(input.Text);
            var parts = results.Select(result => result.Part).ToList();
            if (results.Any(result => result.ConsumedSpan.HasValue))
            {
                foreach (var result in results)
                {
                    if (result.ConsumedSpan is { } span)
                    {
                        input.Cell.Consume(new TextSpan(input.Span.Start + span.Start, span.Length), ConsumptionKind.Exercise);
                    }
                }

                return (input, parts);
            }

            if (input.Cell == expectedCell)
            {
                unresolved = (input, parts);
            }
        }

        return unresolved;
    }

    private static string? GetCellText(WorkoutRowCells row, WorkoutImportProperty property)
    {
        var cell = row.GetExpectedCell(property);
        return string.IsNullOrWhiteSpace(cell?.Text) ? null : cell.Text;
    }

    private static WorkoutImportDraft CreateWorkoutDraft(
        DateTime date, string? sourceName, IReadOnlyList<WorkoutImportRowDraft> rows) =>
        new()
        {
            Date = date,
            SourceReference = sourceName,
            Rows = rows,
            Warnings = CollectExerciseWarnings(rows)
        };

    private static IReadOnlyList<WorkoutImportWarning> CollectExerciseWarnings(
        IReadOnlyList<WorkoutImportRowDraft> rows) =>
        rows.SelectMany(row => row.ExerciseParts
                .Where(part => part.Warning is not null)
                .Select(part => new WorkoutImportWarning
                {
                    Severity = WorkoutImportIssueSeverity.Warning,
                    Message = $"{row.SourceReference}: {part.Warning}"
                }))
            .ToList();

    private static string FormatRowLog(DateTime date, WorkoutImportRowDraft row)
    {
        var message = new StringBuilder($"Interpreted workout row · {date:yyyy-MM-dd} · {row.SourceReference}");
        if (row.Order is int order)
        {
            message.Append($" · order {order}");
        }

        AppendField("Exercise cell", row.SourceText, "  ");
        AppendField("Details", row.Details, "  ");
        AppendField("Notes", row.Notes, "  ");

        if (row.ExerciseParts.Count == 0)
        {
            message.AppendLine().Append("  No exercise parts");
        }

        foreach (var part in row.ExerciseParts)
        {
            message.AppendLine().Append(part.ExerciseName is null
                ? "  - Unresolved exercise"
                : $"  - {part.ExerciseName} ({part.ExerciseId})");

            AppendField(part.MatchedText is null ? "Source" : "Matched",
                part.MatchedText ?? part.SourceText, "    ");
            AppendField("Remaining text", part.RemainingText, "    ");
            AppendField("Attributes", string.Join(", ", part.Attributes.Select(
                attribute => $"{attribute.Type} = {attribute.Value}")), "    ");
            AppendField("Warning", part.Warning, "    ");
        }

        return message.ToString();

        void AppendField(string label, string? value, string indent)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                message.AppendLine().Append(indent).Append(label).Append(": ")
                    .Append(value.ReplaceLineEndings(Environment.NewLine + indent + "  "));
            }
        }
    }
}
