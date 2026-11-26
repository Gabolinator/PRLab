using System.Text;
using PRLab.Application.Models.ImportDraft;

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
            AppendField(exerciseTextLabel, exerciseText, "    ");
            AppendField("Remaining text", part.RemainingText, "    ");
            AppendField("Attributes", attributeText, "    ");
            AppendField("Warning", part.Warning, "    ");
        }

        AppendCellConsumption(message, rowCells);
        return message.ToString();

        void AppendField(string label, string? value, string indent)
        {
            AppendLogField(message, label, value, indent);
        }
    }

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
