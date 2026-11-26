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
        var sourceRows = source.Sheets.SelectMany(sheet => sheet.Rows);
        var rowsByDate = InterpretRowsByDate(sourceRows, columns, matcher, ct);

        logger.Log(nameof(CoachExcelWorkoutImportInterpreter),
            $"Found {rowsByDate.Count} distinct workout dates.");

        IReadOnlyList<WorkoutImportDraft> drafts = rowsByDate
            .Select(entry => CreateWorkoutDraft(entry.Key, source.SourceName, entry.Value))
            .ToList();

        return Task.FromResult(drafts);
    }

    private static ILookup<int, WorkoutImportProperty> ReadColumnMapping(ExcelWorkoutImportSourceProfile profile)
    {
        var exerciseColumn = profile.GetRequiredCell(WorkoutImportProperty.Exercise);
        var orderColumn = GetOptionalColumn(profile, WorkoutImportProperty.Order);
        var detailsColumn = GetOptionalColumn(profile, WorkoutImportProperty.Details);
        var notesColumn = GetOptionalColumn(profile, WorkoutImportProperty.Notes);
        (int Column, WorkoutImportProperty Property)[] columns =
        [
            (0, WorkoutImportProperty.Date),
            (exerciseColumn, WorkoutImportProperty.Exercise),
            (orderColumn, WorkoutImportProperty.Order),
            (detailsColumn, WorkoutImportProperty.Details),
            (notesColumn, WorkoutImportProperty.Notes)
        ];
        var configuredColumns = columns.Where(column => column.Column >= 0);
        return configuredColumns.ToLookup(column => column.Column, column => column.Property);
    }

    private static int GetOptionalColumn(ExcelWorkoutImportSourceProfile profile, WorkoutImportProperty property) =>
        profile.TryGetColumn(property, out var index) ? index : -1;

    private static WorkoutImportRowDraft? InterpretRow(WorkoutRowCells row, WorkoutInterpreterHelper matcher)
    {
        var orderCell = row.GetExpectedCell(WorkoutImportProperty.Order);
        var order = ReadOrder(orderCell);
        var exercise = FindRowExercise(row, matcher);
        if (exercise is null)
        {
            return null;
        }

        var (input, exerciseParts) = exercise.Value;
        var details = GetCellText(row, WorkoutImportProperty.Details);
        var notes = GetCellText(row, WorkoutImportProperty.Notes);
        return new WorkoutImportRowDraft
        {
            SourceText = input.Text,
            SourceReference = input.Cell.SourceReference,
            Order = order,
            Details = details,
            Notes = notes,
            ExerciseParts = exerciseParts
        };
    }

    private static int? ReadOrder(WorkoutRowCell? cell)
    {
        if (cell is null || cell.Consumption.Count > 0 || !int.TryParse(cell.Text, out var order))
        {
            return null;
        }

        var orderSpan = new TextSpan(0, cell.Text.Length);
        cell.Consume(orderSpan, ConsumptionKind.Order);
        return order;
    }

    private static (UnconsumedCellText Input, IReadOnlyList<WorkoutImportExercisePart> Parts)? FindRowExercise(
        WorkoutRowCells row, WorkoutInterpreterHelper matcher)
    {
        var expectedExerciseCell = row.GetExpectedCell(WorkoutImportProperty.Exercise);
        var exerciseInputs = row.GetUnconsumedText()
            .OrderByDescending(input => input.Cell == expectedExerciseCell);
        (UnconsumedCellText Input, IReadOnlyList<WorkoutImportExercisePart> Parts)? unresolved = null;
        foreach (var input in exerciseInputs)
        {
            var inputText = input.Text;
            if (matcher.IsIgnored(inputText))
            {
                continue;
            }

            var results = matcher.InterpretExerciseText(inputText);
            var parts = results.Select(result => result.Part).ToList();
            var hasResolvedExercise = results.Any(result => result.ResolvedSpan.HasValue);
            if (hasResolvedExercise)
            {
                var exerciseCell = input.Cell;
                foreach (var result in results)
                {
                    if (result.ResolvedSpan is { } span)
                    {
                        var exerciseSpan = new TextSpan(input.Span.Start + span.Start, span.Length);
                        exerciseCell.Consume(exerciseSpan, ConsumptionKind.Exercise);
                    }
                }

                return (input, parts);
            }

            if (input.Cell == expectedExerciseCell)
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

            var rowCells = WorkoutRowCells.CreateForDatedRow(row, columns);
            if (rowCells is null)
            {
                continue;
            }

            var date = rowCells.Date;
            if (!rowsByDate.TryGetValue(date, out var rows))
            {
                rows = [];
                rowsByDate.Add(date, rows);
            }

            var rowDraft = InterpretRow(rowCells, matcher);
            if (rowDraft is not null)
            {
                rows.Add(rowDraft);
            }

            if (logger.Enabled)
            {
                var rowLog = WorkoutRowLog.Format(date, row.RowNumber, rowDraft, rowCells);
                logger.Log(nameof(CoachExcelWorkoutImportInterpreter), rowLog);
            }
        }

        return rowsByDate;
    }
}
