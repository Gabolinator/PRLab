using System.Text;
using PRLab.Application.Models.ImportDraft;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal static class WorkoutRowLog
{
    public static string Format(
        DateTime date,
        ExtractedWorkoutRow sourceRow,
        WorkoutImportRowDraft? row,
        WorkoutRowCells rowCells)
    {
        if (row is null)
        {
            var ignoredLabels =
                rowCells.Cells
                    .SelectMany(cell => cell.Consumption
                        .Where(entry => entry.Kind == ConsumptionKind.Ignored)
                        .Select(entry => entry.Span.Read(cell.Text)))
                    .Distinct();
            var reason = string.Join(", ", ignoredLabels);

            var skipped = new StringBuilder($"Skipped workout row · {date:yyyy-MM-dd} · row {sourceRow.RowNumber}");
            if (reason.Length > 0)
            {
                skipped.Append($" · ignored label: {reason}");
            }

            AppendSourceRow(skipped, sourceRow, rowCells);

            return skipped.ToString();
        }

        var message = new StringBuilder($"Interpreted workout row · {date:yyyy-MM-dd} · {row.SourceReference}");
        if (row.Order is int order)
        {
            message.Append($" · order {order}");
        }

        AppendSourceRow(message, sourceRow, rowCells);
        AppendExercises(message, row);
        AppendRowSemantics(message, row.RowSemanticItems);
        AppendUnconsumed(message, row.UnconsumedText);
        AppendWarnings(message, row);

        return message.ToString();
    }

    private static void AppendSourceRow(
        StringBuilder message,
        ExtractedWorkoutRow sourceRow,
        WorkoutRowCells rowCells)
    {
        var lastSourceColumn = sourceRow.Cells.Max(cell => cell.ColumnIndex);
        var sourceValues =
            rowCells.Cells
                .Take(lastSourceColumn + 1)
                .Select(cell => cell.Text);
        var originalRowText = string.Join(" | ", sourceValues);

        AppendLine(message, "  Source row: ", originalRowText);
    }

    private static void AppendExercises(StringBuilder message, WorkoutImportRowDraft row)
    {
        if (row.ExerciseInterpretations.Count == 0)
        {
            return;
        }

        message.AppendLine().Append("  Exercises");
        foreach (var exercise in row.ExerciseInterpretations)
        {
            var part = row.ExerciseParts.FirstOrDefault(part =>
                part.ExerciseId == exercise.Exercise.Id &&
                part.SourceReference == exercise.Source.Reference &&
                part.SourceOffset == exercise.Source.Offset);
            var matchedText = part?.MatchedText ?? exercise.Source.Text;

            message.AppendLine().Append("    ").Append(exercise.Exercise.Name);
            AppendLine(message, "      Matched: ", matchedText);
            AppendSemantics(message, exercise.SemanticItems, "      ");
        }
    }

    private static void AppendRowSemantics(StringBuilder message, IReadOnlyList<WorkoutImportSemanticItem> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        message.AppendLine().Append("  Row and block semantics");
        AppendSemantics(message, items, "    ");
    }

    private static void AppendSemantics(
        StringBuilder message,
        IReadOnlyList<WorkoutImportSemanticItem> items,
        string indent)
    {
        var semanticGroups = items.GroupBy(item => item.Origin);
        foreach (var group in semanticGroups)
        {
            var descriptions =
                group
                    .Select(DescribeSemanticItem)
                    .Distinct();
            if (group.Key.SeriesIndex.HasValue)
            {
                AppendLine(message, indent, string.Join(" · ", descriptions));
                continue;
            }

            foreach (var description in descriptions)
            {
                AppendLine(message, indent, description);
            }
        }
    }

    private static string DescribeSemanticItem(WorkoutImportSemanticItem item) =>
        item switch
        {
            WorkoutTechniqueSemantic technique => $"Technique: {technique.Technique}",
            WorkoutMovementQualitySemantic quality => $"Movement quality: {quality.Quality}",
            WorkoutEquipmentSemantic equipment => $"Equipment: {equipment.Name}",
            WorkoutEquipmentQuantitySemantic quantity => $"Equipment quantity: {quantity.Quantity}",
            WorkoutEquipmentHeightSemantic height => $"Equipment height: {height.Height}",
            WorkoutEquipmentUnitSemantic unit => $"Equipment unit: {unit.Unit}",
            WorkoutAddedLoadSemantic load => $"Added load: {(load.AddedLoad ? "yes" : "no")}",
            WorkoutDistanceUnitSemantic unit => $"Distance unit: {unit.Unit}",
            WorkoutWorkSemantic work => $"Work: {FormatWork(work)}",
            WorkoutLoadSemantic load => $"Load: {FormatLoad(load)}",
            WorkoutRestSemantic rest => $"Rest: {(rest.IsMinimum ? "minimum " : string.Empty)}{FormatDuration(rest.Seconds)}",
            WorkoutTimeCapSemantic cap => $"Time cap: {FormatDuration(cap.Seconds)}",
            WorkoutWorkWindowSemantic window => $"Work window: {FormatWorkMode(window.Mode)} {FormatDuration(window.DurationSeconds)}",
            WorkoutInstructionSemantic { Instruction: WorkoutInstructionKind.SplitAnyhow } => "Instruction: Split anyhow",
            _ => item.Origin.Source.Text
        };

    private static string FormatWork(WorkoutWorkSemantic work)
    {
        var unit = work.TargetType switch
        {
            WorkTargetType.Repetitions => work.Value == 1 ? "rep" : "reps",
            WorkTargetType.DurationSeconds => "seconds",
            WorkTargetType.DistanceMeters => "meters",
            WorkTargetType.TimeUnderTensionSeconds => "seconds under tension",
            _ => work.TargetType.ToString()
        };
        var sets = work.SetCount is { } count
            ? $"{count} {(count == 1 ? "set" : "sets")} × "
            : string.Empty;
        var scope = work.Scope switch
        {
            WorkTargetScope.Total => string.Empty,
            WorkTargetScope.PerSide => " per side",
            _ => $" ({work.Scope})"
        };

        return $"{sets}{work.Value} {unit}{scope}";
    }

    private static string FormatLoad(WorkoutLoadSemantic load)
    {
        var unit = load.Unit switch
        {
            LoadUnit.Kilogram => "kg",
            LoadUnit.Pound => "lb",
            _ => load.Unit.ToString()
        };

        return $"{load.Value} {unit}";
    }

    private static string FormatWorkMode(WorkMode mode) =>
        mode == WorkMode.MaxWorkInTime ? "AMRAP" : mode.ToString();

    private static string FormatDuration(int seconds) =>
        seconds >= 60 && seconds % 60 == 0 ? $"{seconds / 60} min ({seconds} s)" : $"{seconds} s";

    private static void AppendUnconsumed(StringBuilder message, IReadOnlyList<WorkoutImportTextFragment> fragments)
    {
        var remaining =
            fragments
                .Select(fragment => (fragment.SourceReference, Text: CleanUnconsumedText(fragment.Text)))
                .Where(fragment => fragment.Text.Any(char.IsLetterOrDigit))
                .Distinct()
                .ToList();
        if (remaining.Count == 0)
        {
            return;
        }

        message.AppendLine().Append("  Unconsumed");
        foreach (var (sourceReference, text) in remaining)
        {
            var prefix = string.IsNullOrWhiteSpace(sourceReference) ? "    " : $"    {sourceReference} · ";
            AppendLine(message, prefix, text);
        }
    }

    private static string CleanUnconsumedText(string text)
    {
        var trimmed = text.Trim(' ', '\t', '\r', '\n', ',', ';', '|');
        if (trimmed.EndsWith(" +", StringComparison.Ordinal))
        {
            trimmed = trimmed[..^2];
        }

        return trimmed.Trim(' ', '\t', '\r', '\n', '(', ')');
    }

    private static void AppendWarnings(StringBuilder message, WorkoutImportRowDraft row)
    {
        var exerciseWarnings =
            row.ExerciseParts
                .Where(part => part.Warning is not null)
                .Select(part => part.ExerciseName is { } name ? $"{name} · {part.Warning}" : part.Warning!);
        var warnings =
            exerciseWarnings
                .Concat(row.Warnings.Select(warning => warning.Message))
                .Distinct()
                .ToList();
        if (row.WorkWindows.Count > 1)
        {
            warnings.Add("Multiple work windows were found in one row. Exercise grouping needs review.");
        }

        if (warnings.Count == 0)
        {
            return;
        }

        message.AppendLine().Append("  Warnings");
        foreach (var warning in warnings)
        {
            AppendLine(message, "    ", warning);
        }
    }

    private static void AppendLine(StringBuilder message, string prefix, string text) =>
        message.AppendLine().Append(prefix).Append(text.ReplaceLineEndings(Environment.NewLine + "      "));
}
