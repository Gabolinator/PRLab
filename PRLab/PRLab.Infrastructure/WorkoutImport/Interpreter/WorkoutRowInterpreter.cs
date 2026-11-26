using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.WorkoutImport.Interpreter.Extensions;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutRowInterpreter(WorkoutImportContext context, IAppLogger logger)
{
    private readonly ExerciseInterpreter exercises = new(context.Profile, context.Exercises);
    private readonly AttributeInterpreter attributes = new(context.Profile);
    private readonly WorkoutPrescriptionInterpreter prescriptions = new(logger, context.Profile.PreferredMeasurementSystem);
    private readonly string[] separators = context.Profile.Parsing.Separators
        .Where(separator => !string.IsNullOrEmpty(separator)).ToArray();
    private readonly Regex[] ignoredLabels = BuildIgnoredLabelPatterns(context.Profile);
    private readonly IReadOnlyDictionary<string, WorkTargetType> prescriptionHints =
        context.Profile.Aliases.PrescriptionHints.ToDictionary(
            hint => FormatingUtilities.NormalizeNonNullString(hint.Key), hint => hint.Value);

    public WorkoutImportRowDraft? Interpret(WorkoutRowCells row)
    {
        var expectedExerciseCell = row.GetExpectedCell(WorkoutImportProperty.Exercise);
        var isIgnoredRow = expectedExerciseCell is not null && FindIgnoredLabel(expectedExerciseCell.Text).HasValue;
        ConsumeIgnoredLabels(row);
        if (isIgnoredRow)
        {
            return null;
        }

        var orderCell = row.GetExpectedCell(WorkoutImportProperty.Order);
        int? order = orderCell.TryConsumeOrder(out var foundOrder) ? foundOrder : null;
        var exercise = FindRowExercise(row, expectedExerciseCell);
        var detailsCell = row.GetExpectedCell(WorkoutImportProperty.Details);
        var exerciseParts = exercise is null ? [] : InterpretExerciseAttributes(exercise.Fragments, detailsCell);
        var workWindows = InterpretWorkWindows(row);
        if (exercise is null && workWindows.Count == 0)
        {
            return null;
        }

        if (exercise is not null && workWindows.Count > 0)
        {
            var exerciseSource = exercise.Input.Cell.SourceReference;
            var workWindowLabels = workWindows.Where(window => window.SourceReference == exerciseSource)
                .Select(window => window.SourceText)
                .ToHashSet(StringComparer.Ordinal);
            exerciseParts = exerciseParts
                .Where(part => part.MatchedText is not null || !workWindowLabels.Contains(part.SourceText.Trim()))
                .ToList();
        }

        var (rowPrescriptions, warnings) = InterpretPrescriptions(row, exercise);
        var unconsumedText = row.GetUnconsumedText()
            .Select(input => new WorkoutImportTextFragment(input.Cell.SourceReference, input.Text))
            .ToList();
        var details = GetCellText(row, WorkoutImportProperty.Details);
        var notes = GetCellText(row, WorkoutImportProperty.Notes);
        var sourceText = exercise?.Input.Text ?? workWindows[0].SourceText;
        var sourceReference = exercise is null ? workWindows[0].SourceReference : exercise.Input.Cell.SourceReference;
        
        return new WorkoutImportRowDraft
        {
            SourceText = sourceText,
            SourceReference = sourceReference ?? string.Empty,
            Order = order,
            Details = details,
            Notes = notes,
            ExerciseParts = exerciseParts,
            Prescriptions = rowPrescriptions,
            Warnings = warnings,
            WorkWindows = workWindows,
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

    private RowExercise? FindRowExercise(WorkoutRowCells row, WorkoutRowCell? expectedExerciseCell)
    {
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
        IReadOnlyList<ExerciseFragment> fragments, WorkoutRowCell? detailsCell)
    {
        var parts = new List<WorkoutImportExercisePart>();
        var resolvedExerciseCount = fragments.Sum(fragment => fragment.Matches.Count(match => match.Part.ExerciseId.HasValue));
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

                var assignments = result.Assignments.ToList();
                if (fragment.Matches.Count == 1)
                {
                    assignments.AddRange(ConsumeEquipment(fragment.Input.Cell, fragment.Input.Span));
                }

                if (resolvedExerciseCount == 1 && detailsCell is not null && detailsCell != fragment.Input.Cell)
                {
                    var detailsSpan = new TextSpan(0, detailsCell.Text.Length);
                    assignments.AddRange(ConsumeEquipment(detailsCell, detailsSpan));
                }

                var distinctAssignments = assignments.DistinctBy(attribute => (attribute.Type, attribute.Value)).ToList();
                parts.Add(match.Part with
                {
                    SourceOffset = fragment.Input.Span.Start + match.Span.Start,
                    MatchedText = result.MatchedText,
                    RemainingText = result.RemainingText.Length == 0 ? null : result.RemainingText,
                    Attributes = distinctAssignments
                });
            }
        }

        return parts;
    }

    private IReadOnlyList<WorkoutImportAttributeAssignment> ConsumeEquipment(WorkoutRowCell cell, TextSpan region)
    {
        var assignments = new List<WorkoutImportAttributeAssignment>();
        var inputsInRegion = cell.GetUnconsumedText().SelectMany(SplitInput)
            .Where(input => region.Start <= input.Span.Start && input.Span.End <= region.End)
            .ToList();
        foreach (var input in inputsInRegion)
        {
            foreach (var (span, assignment) in attributes.FindEquipment(input.Text))
            {
                var equipmentSpan = span.OffsetBy(input.Span.Start);
                cell.Consume(equipmentSpan, ConsumptionKind.Attribute);
                assignments.Add(assignment);
            }
        }

        return assignments;
    }

    private IReadOnlyList<WorkoutWorkWindowDraft> InterpretWorkWindows(WorkoutRowCells row)
    {
        var drafts = new List<WorkoutWorkWindowDraft>();
        var notesCell = row.GetExpectedCell(WorkoutImportProperty.Notes);
        var workoutInputs = row.GetUnconsumedText()
            .Where(input => input.Cell != notesCell)
            .SelectMany(SplitInput)
            .ToList();
        foreach (var input in workoutInputs)
        {
            var workWindowResults = prescriptions.InterpretWorkWindows(
                input.Text, input.Cell.SourceReference, input.Span.Start);
            foreach (var (draft, span) in workWindowResults)
            {
                var workWindowSpan = span.OffsetBy(input.Span.Start);
                input.Cell.Consume(workWindowSpan, ConsumptionKind.WorkWindow);
                drafts.Add(draft);
            }
        }

        return drafts;
    }

    private (IReadOnlyList<WorkoutImportPrescriptionDraft> Drafts, IReadOnlyList<WorkoutImportWarning> Warnings)
        InterpretPrescriptions(WorkoutRowCells row, RowExercise? exercise)
    {
        var drafts = new List<WorkoutImportPrescriptionDraft>();
        var warnings = new List<WorkoutImportWarning>();
        var fragments = exercise?.Fragments ?? [];
        var resolvedMatches = fragments.SelectMany(fragment => fragment.Matches
                .Where(match => match.Part.ExerciseId.HasValue)
                .Select(match => (Fragment: fragment, Match: match)))
            .ToList();
        var detailsCell = row.GetExpectedCell(WorkoutImportProperty.Details);
        var notesCell = row.GetExpectedCell(WorkoutImportProperty.Notes);
        var remainingInputs = row.GetUnconsumedText()
            .Where(input => input.Cell != notesCell && (input.Cell == detailsCell || input.Cell == exercise?.Input.Cell))
            .SelectMany(SplitInput)
            .ToList();
        foreach (var input in remainingInputs)
        {
            var fragment = fragments.FirstOrDefault(candidate => candidate.Input.Cell == input.Cell &&
                candidate.Input.Span.Start <= input.Span.Start && candidate.Input.Span.End >= input.Span.End);
            var fallbackContext = input.Cell == detailsCell && exercise?.Input.Cell != input.Cell && resolvedMatches.Count == 1
                ? CreatePrescriptionContext(resolvedMatches[0].Fragment, resolvedMatches[0].Match)
                : null;
            var inputText = input.Text;
            prescriptions.LogFragment(inputText, input.Cell.SourceReference, input.Span.Start);
            var tokens = WorkoutPrescriptionSyntax.Scan(inputText, context.Profile.Parsing.UseCompactRepLoadSyntax);
            foreach (var token in tokens)
            {
                var prescriptionSpan = token.Span.OffsetBy(input.Span.Start);
                var requiresFollowingExercise = token is RawWorkToken { RequiresFollowingExercise: true };
                var exerciseContext = fragment is not null
                    ? FindFragmentExercise(fragment, prescriptionSpan, requiresFollowingExercise)
                    : requiresFollowingExercise ? null : fallbackContext;
                var result = prescriptions.InterpretToken(
                    token, inputText, input.Cell.SourceReference, input.Span.Start, exerciseContext);
                if (result.Warning is { } warning)
                {
                    var sourceText = prescriptionSpan.Read(input.Cell.Text);
                    warnings.Add(new WorkoutImportWarning
                    {
                        Severity = WorkoutImportIssueSeverity.Warning,
                        Message = $"{input.Cell.SourceReference}: '{sourceText}' — {warning}"
                    });
                }

                if (result.Drafts.Count > 0)
                {
                    input.Cell.Consume(prescriptionSpan, ConsumptionKind.Prescription);
                    drafts.AddRange(result.Drafts);
                }
            }
        }

        return (drafts, warnings);
    }

    private ExercisePrescriptionContext? FindFragmentExercise(
        ExerciseFragment fragment, TextSpan prescriptionSpan, bool requiresFollowingExercise)
    {
        var tokenStart = prescriptionSpan.Start - fragment.Input.Span.Start;
        var tokenEnd = prescriptionSpan.End - fragment.Input.Span.Start;
        var followingMatch = fragment.Matches.FirstOrDefault(match => match.Span.Start >= tokenEnd);
        if (requiresFollowingExercise)
        {
            if (followingMatch is null)
            {
                return null;
            }

            var gap = fragment.Input.Text[tokenEnd..followingMatch.Span.Start];
            return string.IsNullOrWhiteSpace(gap) ? CreatePrescriptionContext(fragment, followingMatch) : null;
        }

        var precedingMatch = fragment.Matches.LastOrDefault(match => match.Span.End <= tokenStart);
        var match = precedingMatch is null ? followingMatch : followingMatch is null ? precedingMatch : null;
        return match is null ? null : CreatePrescriptionContext(fragment, match);
    }

    private ExercisePrescriptionContext? CreatePrescriptionContext(ExerciseFragment fragment, ExerciseTextMatch match)
    {
        var exercise = context.Exercises.FirstOrDefault(reference => reference.Id == match.Part.ExerciseId);
        if (exercise is null)
        {
            return null;
        }

        var matchText = match.Span.Read(fragment.Input.Text);
        var hintKey = FormatingUtilities.NormalizeNonNullString(matchText);
        WorkTargetType? hint = prescriptionHints.TryGetValue(hintKey, out var configuredHint) ? configuredHint : null;
        var exerciseOffset = fragment.Input.Span.Start + match.Span.Start;
        return new ExercisePrescriptionContext(exercise, hint, fragment.Input.Cell.SourceReference, exerciseOffset);
    }

    private IEnumerable<UnconsumedCellText> SplitInput(UnconsumedCellText input)
    {
        var text = input.Text;
        var fragments = WorkoutTextMatching.Split(text, separators);
        foreach (var span in fragments)
        {
            var fragmentText = span.Read(text);
            if (!string.IsNullOrWhiteSpace(fragmentText))
            {
                yield return new UnconsumedCellText(input.Cell, span.OffsetBy(input.Span.Start));
            }
        }
    }

    private void ConsumeIgnoredLabels(WorkoutRowCells row)
    {
        if (ignoredLabels.Length == 0)
        {
            return;
        }

        var fragments = row.GetUnconsumedText().SelectMany(SplitInput).ToList();
        foreach (var fragment in fragments)
        {
            if (FindIgnoredLabel(fragment.Text) is { } labelSpan)
            {
                var sourceSpan = labelSpan.OffsetBy(fragment.Span.Start);
                fragment.Cell.Consume(sourceSpan, ConsumptionKind.Ignored);
            }
        }
    }

    private TextSpan? FindIgnoredLabel(string text)
    {
        var trimmedText = text.TrimStart();
        var matches = ignoredLabels.Select(pattern => pattern.Match(trimmedText));
        var longestMatch = matches.DefaultIfEmpty().MaxBy(match => match?.Length ?? 0);
        if (longestMatch is null || !longestMatch.Success)
        {
            return null;
        }

        var labelStart = text.Length - trimmedText.Length;
        return new TextSpan(labelStart, longestMatch.Length);
    }

    private sealed record RowExercise(UnconsumedCellText Input, IReadOnlyList<ExerciseFragment> Fragments);

    private sealed record ExerciseFragment(UnconsumedCellText Input, IReadOnlyList<ExerciseTextMatch> Matches);
}
