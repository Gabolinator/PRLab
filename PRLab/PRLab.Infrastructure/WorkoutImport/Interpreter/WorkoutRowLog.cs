using System.Text;
using PRLab.Application.Models.ImportDraft;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal static class WorkoutRowLog
{
    public static string Format(
        DateTime date, int rowNumber, WorkoutImportRowDraft? row, WorkoutRowCells rowCells)
    {
        if (row is null)
        {
            var skipped = new StringBuilder($"Skipped workout row · {date:yyyy-MM-dd} · row {rowNumber}");
            AppendCellConsumption(skipped, rowCells);
            return skipped.ToString();
        }

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

            var exerciseTextLabel = part.MatchedText is null ? "Source" : "Matched";
            var exerciseText = part.MatchedText ?? part.SourceText;
            var exerciseAttributes = part.Attributes.Select(attribute => $"{attribute.Type} = {attribute.Value}");
            var attributeText = string.Join(", ", exerciseAttributes);
            var expectedAttributes = part.ExpectedAttributes.Select(attribute => $"{attribute.Type} = {attribute.Value}");
            var expectedAttributeText = string.Join(", ", expectedAttributes);
            AppendField(exerciseTextLabel, exerciseText, "    ");
            AppendField("Remaining text", part.RemainingText, "    ");
            AppendField("Attributes", attributeText, "    ");
            AppendField("Expected attributes", expectedAttributeText, "    ");
            AppendField("Warning", part.Warning, "    ");
        }

        AppendResolvedPrescriptions(message, row);
        AppendCellConsumption(message, rowCells);
        return message.ToString();

        void AppendField(string label, string? value, string indent)
        {
            AppendLogField(message, label, value, indent);
        }
    }

    private static void AppendResolvedPrescriptions(StringBuilder message, WorkoutImportRowDraft row)
    {
        if (row.WorkWindows.Count == 0 && row.Prescriptions.Count == 0)
        {
            return;
        }

        message.AppendLine().Append("  Resolved prescriptions");
        foreach (var window in row.WorkWindows)
        {
            var mode = window.Mode == WorkMode.MaxWorkInTime ? "AMRAP (MaxWorkInTime)" : window.Mode.ToString();
            var duration = FormatDuration(window.DurationSeconds);
            var result = $"{window.SourceText} → {mode}, {duration} · {window.SourceReference}";
            AppendLogField(message, "Work window", result, "    ");
        }

        foreach (var prescription in row.Prescriptions)
        {
            var step = prescription.Step;
            var details = FormatPrescription(prescription);
            var result = $"{step.SourceText} → {details} · {prescription.SourceReference}";
            AppendLogField(message, "Prescription", result, "    ");
        }
    }

    private static string FormatPrescription(WorkoutImportPrescriptionDraft prescription)
    {
        var step = prescription.Step;
        var details = new List<string>();
        if (step.ExerciseName is { } name)
        {
            details.Add(name);
        }

        if (step.WorkValue is { } value && step.WorkTargetType is { } target)
        {
            var work = target == WorkTargetType.DurationSeconds ? $"{value} seconds" : $"{value} {target}";
            details.Add(step.Scope == WorkTargetScope.Total ? work : $"{work} ({step.Scope})");
        }

        if (step.SetCount is { } sets)
        {
            details.Add($"sets = {sets}");
        }

        if (step.LoadValue is { } load)
        {
            details.Add($"load = {load} {step.LoadUnit}");
        }

        if (step.RestSeconds is { } rest)
        {
            details.Add($"rest = {FormatDuration(rest)}");
        }

        if (step.MinimumRestSeconds is { } minimumRest)
        {
            details.Add($"minimum rest = {FormatDuration(minimumRest)}");
        }

        if (prescription.TimeCapSeconds is { } cap)
        {
            details.Add($"time cap = {FormatDuration(cap)}");
        }

        if (step.Notes is { } notes)
        {
            details.Add(notes);
        }

        return details.Count == 0 ? step.StepKind.ToString() : string.Join(", ", details);
    }

    private static string FormatDuration(int seconds) =>
        seconds >= 60 && seconds % 60 == 0 ? $"{seconds / 60} min ({seconds} s)" : $"{seconds} s";

    private static void AppendCellConsumption(StringBuilder message, WorkoutRowCells row)
    {
        message.AppendLine().Append($"  Cell consumption (date column {row.DateColumnIndex + 1}, offset {row.ColumnOffset})");
        foreach (var cell in row.Cells)
        {
            var remainingText = cell.GetUnconsumedText().ToList();
            var state = string.IsNullOrWhiteSpace(cell.Text) ? "blank"
                : cell.Consumption.Count == 0 ? "unconsumed"
                : remainingText.Count == 0 ? "consumed"
                : "partially consumed";
            var expected = cell.ExpectedProperties.Count == 0
                ? "none"
                : string.Join(", ", cell.ExpectedProperties);

            message.AppendLine().Append($"    Column {cell.ColumnIndex + 1} · {state} · expected {expected}");
            if (cell.SourceReference is not null)
            {
                message.Append($" · {cell.SourceReference}");
            }

            var consumptionInTextOrder = cell.Consumption.OrderBy(entry => entry.Span.Start);
            foreach (var entry in consumptionInTextOrder)
            {
                var consumedText = cell.Text.Substring(entry.Span.Start, entry.Span.Length);
                AppendLogField(message, $"Consumed as {entry.Kind}", consumedText, "      ");
            }

            foreach (var remaining in remainingText)
            {
                AppendLogField(message, "Unconsumed", remaining.Text, "      ");
            }
        }
    }

    private static void AppendLogField(StringBuilder message, string label, string? value, string indent)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            message.AppendLine().Append(indent).Append(label).Append(": ")
                .Append(value.ReplaceLineEndings(Environment.NewLine + indent + "  "));
        }
    }
}
