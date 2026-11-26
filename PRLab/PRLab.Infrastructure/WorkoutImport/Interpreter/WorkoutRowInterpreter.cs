using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.WorkoutImport.Interpreter.Extensions;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutRowInterpreter(WorkoutImportContext context, IAppLogger logger)
{
    private readonly ExerciseInterpreter exercises = new(context.Profile, context.Exercises);
    private readonly AttributeInterpreter attributes = new(context.Profile);
    private readonly WorkoutPrescriptionInterpreter prescriptions = new(logger);
    private readonly string[] separators = context.Profile.Parsing.Separators
        .Where(separator => !string.IsNullOrEmpty(separator)).ToArray();
    private readonly Regex[] ignoredLabels = BuildIgnoredLabelPatterns(context.Profile);

    public WorkoutImportRowDraft? Interpret(WorkoutRowCells row)
    {
        var exercise = FindRowExercise(row);
        if (exercise is null)
        {
            return null;
        }

        var orderCell = row.GetExpectedCell(WorkoutImportProperty.Order);
        int? order = orderCell.TryConsumeOrder(out var foundOrder) ? foundOrder : null;
        
        var exerciseParts = InterpretExerciseAttributes(exercise.Fragments);
        var rowPrescriptions = InterpretPrescriptions(row);
        var unconsumedText = row.GetUnconsumedText()
            .Select(input => new WorkoutImportTextFragment(input.Cell.SourceReference, input.Text))
            .ToList();
        var details = GetCellText(row, WorkoutImportProperty.Details);
        var notes = GetCellText(row, WorkoutImportProperty.Notes);
        var sourceReference = exercise.Input.Cell.SourceReference ?? string.Empty;
        
        return new WorkoutImportRowDraft
        {
            SourceText = exercise.Input.Text,
            SourceReference = sourceReference,
            Order = order,
            Details = details,
            Notes = notes,
            ExerciseParts = exerciseParts,
            Prescriptions = rowPrescriptions,
            UnconsumedText = unconsumedText
        };
    }

    private static Regex[] BuildIgnoredLabelPatterns(WorkoutImportProfile profile) =>
        profile.Parsing.IgnoredLabels
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => WorkoutTextMatching.CreatePhrasePattern(label, "^", @"(?=[^\S\r\n]*(?:$|\r|\n))"))
            .ToArray();

    private static string? GetCellText(WorkoutRowCells row, WorkoutImportProperty property)
    {
        var cell = row.GetExpectedCell(property);
        return string.IsNullOrWhiteSpace(cell?.Text) ? null : cell.Text;
    }

    private static WorkoutImportExercisePart CreateUnresolvedPart(string text)
    {
        var unmatchedText = text.Trim();
        return new WorkoutImportExercisePart
        {
            SourceText = text,
            Warning = $"No catalog exercise matches '{unmatchedText}'."
        };
    }

    private RowExercise? FindRowExercise(WorkoutRowCells row)
    {
        var expectedExerciseCell = row.GetExpectedCell(WorkoutImportProperty.Exercise);
        if (expectedExerciseCell is not null && IsIgnored(expectedExerciseCell.Text))
        {
            return null;
        }

        var exerciseInputsInPriorityOrder = row.GetUnconsumedText()
            .OrderByDescending(input => input.Cell == expectedExerciseCell);
        RowExercise? unresolved = null;

        foreach (var input in exerciseInputsInPriorityOrder)
        {
            if (TryGetExercisePart(input, out var exerciseResult))
            {
                return exerciseResult;
            }

            if (input.Cell == expectedExerciseCell && exerciseResult is not null)
            {
                unresolved = exerciseResult;
            }
        }

        return unresolved;
    }

    private bool TryGetExercisePart(UnconsumedCellText input, out RowExercise? exerciseResult)
    {
        exerciseResult = null;
        if (IsIgnored(input.Text))
        {
            return false;
        }

        var fragments = new List<ExerciseFragment>();
        foreach (var fragment in SplitInput(input))
        {
            var text = fragment.Text;
            var matches = exercises.Interpret(text);
            fragments.Add(new ExerciseFragment(fragment, matches));

            var resolvedMatches = matches.Where(match => match.Part.ExerciseId.HasValue);
            foreach (var match in resolvedMatches)
            {
                var exerciseSpan = match.Span.OffsetBy(fragment.Span.Start);
                fragment.Cell.Consume(exerciseSpan, ConsumptionKind.Exercise);
            }
        }

        exerciseResult = new RowExercise(input, fragments);
        return fragments.Any(fragment => fragment.Matches.Any(match => match.Part.ExerciseId.HasValue));
    }

    private IReadOnlyList<WorkoutImportExercisePart> InterpretExerciseAttributes(
        IReadOnlyList<ExerciseFragment> fragments)
    {
        var parts = new List<WorkoutImportExercisePart>();
        foreach (var fragment in fragments)
        {
            var text = fragment.Input.Text;
            if (fragment.Matches.Count == 0)
            {
                parts.Add(CreateUnresolvedPart(text));
                continue;
            }

            for (var index = 0; index < fragment.Matches.Count; index++)
            {
                var match = fragment.Matches[index];
                if (!match.Part.ExerciseId.HasValue)
                {
                    parts.Add(match.Part);
                    continue;
                }

                var nextIndex = index + 1;
                var nextStart = nextIndex < fragment.Matches.Count
                    ? fragment.Matches[nextIndex].Span.Start
                    : text.Length;
                var result = attributes.Interpret(text, match.Span, nextStart, index == 0);
                foreach (var span in result.Spans)
                {
                    var attributeSpan = span.OffsetBy(fragment.Input.Span.Start);
                    fragment.Input.Cell.Consume(attributeSpan, ConsumptionKind.Attribute);
                }

                parts.Add(match.Part with
                {
                    MatchedText = result.MatchedText,
                    RemainingText = result.RemainingText.Length == 0 ? null : result.RemainingText,
                    Attributes = result.Assignments
                });
            }
        }

        return parts;
    }

    private IReadOnlyList<WorkoutImportPrescriptionDraft> InterpretPrescriptions(WorkoutRowCells row)
    {
        var drafts = new List<WorkoutImportPrescriptionDraft>();
        var remainingInputs = row.GetUnconsumedText().SelectMany(SplitInput).ToList();
        foreach (var input in remainingInputs)
        {
            var prescriptionResults = prescriptions.Interpret(input.Text, input.Cell.SourceReference, input.Span.Start);
            foreach (var (draft, span) in prescriptionResults)
            {
                var prescriptionSpan = span.OffsetBy(input.Span.Start);
                input.Cell.Consume(prescriptionSpan, ConsumptionKind.Prescription);
                drafts.Add(draft);
            }
        }

        return drafts;
    }

    private IEnumerable<UnconsumedCellText> SplitInput(UnconsumedCellText input)
    {
        var text = input.Text;
        var fragments = WorkoutTextMatching.Split(text, separators);
        foreach (var span in fragments)
        {
            var fragmentText = span.Read(text);
            if (!string.IsNullOrWhiteSpace(fragmentText) && !IsIgnored(fragmentText))
            {
                yield return new UnconsumedCellText(input.Cell, span.OffsetBy(input.Span.Start));
            }
        }
    }

    private bool IsIgnored(string text)
    {
        var trimmedText = text.TrimStart();
        return ignoredLabels.Any(pattern => pattern.IsMatch(trimmedText));
    }

    private sealed record RowExercise(UnconsumedCellText Input, IReadOnlyList<ExerciseFragment> Fragments);

    private sealed record ExerciseFragment(UnconsumedCellText Input, IReadOnlyList<ExerciseTextMatch> Matches);
}
