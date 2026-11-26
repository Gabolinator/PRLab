using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;
using PRLab.Infrastructure.WorkoutImport.Interpreter.Extensions;

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
        var columns = excelProfile.GetCoachColumnMapping();
        var rowInterpreter = new WorkoutRowInterpreter(context, logger);
        var sourceRows = source.Sheets.SelectMany(sheet => sheet.Rows);
        var rowsByDate = InterpretRowsByDate(sourceRows, columns, rowInterpreter, ct);

        logger.Log(nameof(CoachExcelWorkoutImportInterpreter),
            $"Found {rowsByDate.Count} distinct workout dates.");

        IReadOnlyList<WorkoutImportDraft> drafts = rowsByDate
            .Select(entry => CreateWorkoutDraft(entry.Key, source.SourceName, entry.Value))
            .ToList();

        return Task.FromResult(drafts);
    }

    private static WorkoutImportDraft CreateWorkoutDraft(
        DateTime date, string? sourceName, IReadOnlyList<WorkoutImportRowDraft> rows) =>
        new()
        {
            Date = date,
            SourceReference = sourceName,
            Blocks = CreateWorkoutBlocks(rows),
            Rows = rows,
            Warnings = CollectImportWarnings(rows)
        };

    private static IReadOnlyList<WorkoutBlockImportDraft> CreateWorkoutBlocks(
        IReadOnlyList<WorkoutImportRowDraft> rows)
    {
        var blockRows = rows.Where(row => row.WorkWindows.Count == 1);
        return blockRows.Select(CreateWorkoutBlock).ToList();
    }

    private static WorkoutBlockImportDraft CreateWorkoutBlock(WorkoutImportRowDraft row)
    {
        var workWindow = row.WorkWindows[0];
        var resolvedExercises = row.ExerciseParts.Where(part => part.ExerciseId.HasValue);
        var steps = resolvedExercises.Select(part => new WorkoutStepImportDraft
        {
            SourceText = part.SourceText,
            ExerciseName = part.ExerciseName,
            StepKind = WorkoutStepKind.Exercise,
            Confidence = 1
        }).ToList();
        var segment = new WorkoutSegmentImportDraft
        {
            Name = workWindow.SourceText,
            WorkMode = workWindow.Mode,
            WorkWindowSeconds = workWindow.DurationSeconds,
            Steps = steps
        };
        return new WorkoutBlockImportDraft
        {
            Name = workWindow.SourceText,
            Order = row.Order,
            Segments = [segment]
        };
    }

    private static IReadOnlyList<WorkoutImportWarning> CollectImportWarnings(
        IReadOnlyList<WorkoutImportRowDraft> rows)
    {
        var exerciseWarnings = rows.SelectMany(row => row.ExerciseParts
                .Where(part => part.Warning is not null)
                .Select(part => new WorkoutImportWarning
                {
                    Severity = WorkoutImportIssueSeverity.Warning,
                    Message = $"{row.SourceReference}: {part.Warning}"
                }));
        var ambiguousBlockRows = rows.Where(row => row.WorkWindows.Count > 1);
        var blockWarnings = ambiguousBlockRows.Select(row => new WorkoutImportWarning
        {
            Severity = WorkoutImportIssueSeverity.Warning,
            Message = $"{row.SourceReference}: Multiple work windows were found in one row. Exercise grouping needs review."
        });
        return exerciseWarnings.Concat(blockWarnings).ToList();
    }

    private SortedDictionary<DateTime, List<WorkoutImportRowDraft>> InterpretRowsByDate(
        IEnumerable<ExtractedWorkoutRow> sourceRows,
        ILookup<int, WorkoutImportProperty> columns,
        WorkoutRowInterpreter rowInterpreter,
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

            var rowDraft = rowInterpreter.Interpret(rowCells);
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
