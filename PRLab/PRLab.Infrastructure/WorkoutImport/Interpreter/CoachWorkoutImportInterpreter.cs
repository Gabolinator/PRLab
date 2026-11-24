using System.Text.Json;
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
        var dateIndex = excelProfile.GetRequiredCell(WorkoutImportProperty.Date);
        var exerciseIndex = excelProfile.GetRequiredCell(WorkoutImportProperty.Exercise);
        var orderIndex = GetOptionalCell(WorkoutImportProperty.Order);
        var detailsIndex = GetOptionalCell(WorkoutImportProperty.Details);
        var notesIndex = GetOptionalCell(WorkoutImportProperty.Notes);

        var matcher = new WorkoutInterpreterHelper(profile, context.Exercises);
        var rowsByDate = new SortedDictionary<DateTime, List<WorkoutImportRowDraft>>();

        foreach (var row in source.Sheets.SelectMany(sheet => sheet.Rows))
        {
            ct.ThrowIfCancellationRequested();

            if (!WorkoutInterpreterHelper.TryReadDate(GetCell(row, dateIndex)?.Value, out var date))
            {
                continue;
            }

            if (!rowsByDate.TryGetValue(date, out var rows))
            {
                rows = [];
                rowsByDate.Add(date, rows);
            }

            var exerciseCell = GetCell(row, exerciseIndex);
            if (exerciseCell is null || matcher.IsIgnored(exerciseCell.Value))
            {
                continue;
            }

            var rowDraft = new WorkoutImportRowDraft
            {
                SourceText = exerciseCell.Value,
                SourceReference = exerciseCell.SourceReference,
                Order = int.TryParse(GetCell(row, orderIndex)?.Value, out var order) ? order : null,
                Details = GetCell(row, detailsIndex)?.Value,
                Notes = GetCell(row, notesIndex)?.Value,
                ExerciseParts = matcher.InterpretExerciseCell(exerciseCell.Value)
            };

            rows.Add(rowDraft);
            if (logger.Enabled)
            {
                logger.Log(nameof(CoachExcelWorkoutImportInterpreter),
                    $"Interpreted workout row for {date:yyyy-MM-dd}: {JsonSerializer.Serialize(rowDraft)}");
            }
        }

        logger.Log(nameof(CoachExcelWorkoutImportInterpreter),
            $"Found {rowsByDate.Count} distinct workout dates.");

        IReadOnlyList<WorkoutImportDraft> drafts = rowsByDate
            .Select(entry => new WorkoutImportDraft
            {
                Date = entry.Key,
                SourceReference = source.SourceName,
                Rows = entry.Value,
                Warnings = entry.Value
                    .SelectMany(row => row.ExerciseParts
                        .Where(part => part.Warning is not null)
                        .Select(part => new WorkoutImportWarning
                        {
                            Severity = WorkoutImportIssueSeverity.Warning,
                            Message = $"{row.SourceReference}: {part.Warning}"
                        }))
                    .ToList()
            })
            .ToList();

        return Task.FromResult(drafts);

        int GetOptionalCell(WorkoutImportProperty property) =>
            excelProfile.TryGetColumn(property, out var index) ? index : -1;
    }

    private static ExtractedWorkoutCell? GetCell(ExtractedWorkoutRow row, int cellIndex) =>
        cellIndex >= 0 && cellIndex < row.Cells.Count ? row.Cells[cellIndex] : null;
}
