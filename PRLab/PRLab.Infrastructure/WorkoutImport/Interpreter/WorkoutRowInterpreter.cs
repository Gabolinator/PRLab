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

    private readonly WorkoutPrescriptionInterpreter prescriptions = new(logger, context.Profile);

    private readonly string[] separators =
        context.Profile.Parsing.Separators
            .Where(separator => !string.IsNullOrEmpty(separator))
            .ToArray();

    private readonly Regex[] ignoredLabels = BuildIgnoredLabelPatterns(context.Profile);

    private readonly IReadOnlyDictionary<string, WorkTargetType> prescriptionHints =
        context.Profile.Aliases.PrescriptionHints
            .ToDictionary(
                hint => FormatingUtilities.NormalizeNonNullString(hint.Key),
                hint => hint.Value);

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
        var semanticItems = new List<WorkoutImportSemanticItem>();
        var exerciseParts = exercise is null ? [] : RecognizeExerciseQualifiers(exercise.Fragments, detailsCell, semanticItems);
        var workWindows = RecognizeWorkWindows(row);
        semanticItems.AddRange(workWindows);

        if (exercise is null && workWindows.Count == 0)
        {
            return null;
        }

        if (exercise is not null && workWindows.Count > 0)
        {
            var exerciseSource = exercise.Input.Cell.SourceReference;
            var workWindowLabels =
                workWindows
                    .Where(window => window.Origin.Source.Reference == exerciseSource)
                    .Select(window => window.Origin.Source.Text)
                    .ToHashSet(StringComparer.Ordinal);
            exerciseParts =
                exerciseParts
                    .Where(part => part.MatchedText is not null || !workWindowLabels.Contains(part.SourceText.Trim()))
                    .ToList();
        }

        var warnings = RecognizePrescriptions(row, exercise, semanticItems);
        var exerciseInterpretations = CreateExerciseInterpretations(exercise, semanticItems);
        var rowSemanticItems =
            semanticItems
                .Where(item => item.Origin.Target is not WorkoutExerciseSemanticTarget)
                .ToList();

        var unconsumedText =
            row.GetUnconsumedText()
                .Select(input => new WorkoutImportTextFragment(input.Cell.SourceReference, input.Text)
                {
                    SourceOffset = input.Span.Start
                })
                .ToList();

        var details = GetCellText(row, WorkoutImportProperty.Details);
        var notes = GetCellText(row, WorkoutImportProperty.Notes);
        var sourceText = exercise?.Input.Text ?? workWindows[0].Origin.Source.Text;
        var sourceReference = exercise is null ? workWindows[0].Origin.Source.Reference : exercise.Input.Cell.SourceReference;

        return new WorkoutImportRowDraft
        {
            SourceText = sourceText,
            SourceReference = sourceReference ?? string.Empty,
            Order = order,
            Details = details,
            Notes = notes,
            ExerciseParts = exerciseParts,
            ExerciseInterpretations = exerciseInterpretations,
            RowSemanticItems = rowSemanticItems,
            Warnings = warnings,
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

    private static WorkoutSemanticSource CreateSemanticSource(UnconsumedCellText input) =>
        new(input.Text, input.Cell.SourceReference, input.Span.Start, input.Cell.ColumnIndex);

    private RowExercise? FindRowExercise(WorkoutRowCells row, WorkoutRowCell? expectedExerciseCell)
    {
        var exerciseInputsInPriorityOrder =
            row.GetUnconsumedText()
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

    private IReadOnlyList<WorkoutImportExercisePart> RecognizeExerciseQualifiers(
        IReadOnlyList<ExerciseFragment> fragments,
        WorkoutRowCell? detailsCell,
        List<WorkoutImportSemanticItem> semanticItems)
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

                var source = CreateSemanticSource(fragment.Input);
                var exerciseOffset = fragment.Input.Span.Start + match.Span.Start;
                var target = new WorkoutExerciseSemanticTarget(match.Part.ExerciseId.Value, source.Reference, exerciseOffset);
                var result = attributes.Recognize(source, target, match.Span, nextStart, index == 0);

                foreach (var span in result.Spans)
                {
                    var attributeSpan = span.OffsetBy(fragment.Input.Span.Start);
                    fragment.Input.Cell.Consume(attributeSpan, ConsumptionKind.Attribute);
                }

                semanticItems.AddRange(result.Items);

                if (fragment.Matches.Count == 1)
                {
                    semanticItems.AddRange(RecognizeStandaloneAttributes(fragment.Input.Cell, fragment.Input.Span, target));
                }

                if (resolvedExerciseCount == 1 && detailsCell is not null && detailsCell != fragment.Input.Cell)
                {
                    var detailsSpan = new TextSpan(0, detailsCell.Text.Length);
                    semanticItems.AddRange(RecognizeStandaloneAttributes(detailsCell, detailsSpan, target));
                }

                parts.Add(match.Part with
                {
                    SourceReference = source.Reference,
                    SourceOffset = exerciseOffset,
                    MatchedText = result.MatchedText,
                    RemainingText = result.RemainingText.Length == 0 ? null : result.RemainingText
                });
            }
        }

        return parts;
    }

    private IReadOnlyList<WorkoutImportSemanticItem> RecognizeStandaloneAttributes(
        WorkoutRowCell cell,
        TextSpan region,
        WorkoutExerciseSemanticTarget target)
    {
        var items = new List<WorkoutImportSemanticItem>();
        var inputsInRegion =
            cell.GetUnconsumedText()
                .SelectMany(SplitInput)
                .Where(input => region.Start <= input.Span.Start && input.Span.End <= region.End)
                .ToList();

        foreach (var input in inputsInRegion)
        {
            var source = CreateSemanticSource(input);
            var standaloneItems =
                attributes.FindPauses(source, target)
                    .Cast<WorkoutImportSemanticItem>()
                    .Concat(attributes.FindEquipment(source, target))
                    .OrderBy(item => item.Origin.Source.Offset)
                    .ThenByDescending(item => item.Origin.Source.Length);
            var previousEnd = input.Span.Start;
            foreach (var item in standaloneItems)
            {
                var matchedSource = item.Origin.Source;
                if (matchedSource.Offset < previousEnd)
                {
                    continue;
                }

                var matchedSpan = new TextSpan(matchedSource.Offset, matchedSource.Length);
                cell.Consume(matchedSpan, ConsumptionKind.Attribute);
                items.Add(item);
                previousEnd = matchedSpan.End;
            }
        }

        return items;
    }

    private IReadOnlyList<WorkoutWorkWindowSemantic> RecognizeWorkWindows(WorkoutRowCells row)
    {
        var items = new List<WorkoutWorkWindowSemantic>();
        var notesCell = row.GetExpectedCell(WorkoutImportProperty.Notes);
        var workoutInputs =
            row.GetUnconsumedText()
                .Where(input => input.Cell != notesCell)
                .SelectMany(SplitInput)
                .ToList();

        foreach (var input in workoutInputs)
        {
            var source = CreateSemanticSource(input);
            foreach (var item in prescriptions.RecognizeWorkWindows(source))
            {
                var matchedSource = item.Origin.Source;
                var workWindowSpan = new TextSpan(matchedSource.Offset, matchedSource.Length);
                input.Cell.Consume(workWindowSpan, ConsumptionKind.WorkWindow);
                items.Add(item);
            }
        }

        return items;
    }

    private IReadOnlyList<WorkoutImportWarning> RecognizePrescriptions(
        WorkoutRowCells row,
        RowExercise? exercise,
        List<WorkoutImportSemanticItem> semanticItems)
    {
        var warnings = new List<WorkoutImportWarning>();
        var fragments = exercise?.Fragments ?? [];
        var resolvedMatches =
            fragments
                .SelectMany(fragment => fragment.Matches
                    .Where(match => match.Part.ExerciseId.HasValue)
                    .Select(match => (Fragment: fragment, Match: match)))
                .ToList();

        var detailsCell = row.GetExpectedCell(WorkoutImportProperty.Details);
        var notesCell = row.GetExpectedCell(WorkoutImportProperty.Notes);
        var externalLoadEquipmentTargets =
            semanticItems
                .OfType<WorkoutEquipmentSemantic>()
                .Where(item => WorkoutImportEquipment.SupportsExternalLoad(item.Name))
                .Select(item => item.Origin.Target)
                .ToHashSet();
        var remainingInputs =
            row.GetUnconsumedText()
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
            var source = CreateSemanticSource(input);
            var tokens = WorkoutPrescriptionSyntax.Scan(inputText, context.Profile.Parsing.UseCompactRepLoadSyntax).ToList();
            var instructions = tokens.OfType<RawInstructionToken>()
                .Select(instruction => instruction.Instruction)
                .ToHashSet();
            WorkoutWorkSemantic? repeatedWork = null;
            var previousTokenEnd = 0;
            foreach (var token in tokens)
            {
                var prescriptionSpan = token.Span.OffsetBy(input.Span.Start);
                var requiresFollowingExercise = token is RawWorkToken { RequiresFollowingExercise: true };
                var exerciseContext = fragment is not null
                    ? FindFragmentExercise(fragment, prescriptionSpan, token)
                    : requiresFollowingExercise ? null : fallbackContext;
                if (exerciseContext is not null)
                {
                    var exerciseTarget = new WorkoutExerciseSemanticTarget(
                        exerciseContext.Exercise.Id,
                        exerciseContext.SourceReference,
                        exerciseContext.SourceOffset);
                    exerciseContext = exerciseContext with
                    {
                        HasExternalLoadEquipment = externalLoadEquipmentTargets.Contains(exerciseTarget)
                    };
                }

                var gap = inputText[previousTokenEnd..token.Span.Start];
                var cadenceWork = string.IsNullOrWhiteSpace(gap) ? repeatedWork : null;
                var result = prescriptions.RecognizeToken(token, source, exerciseContext, instructions, cadenceWork);
                repeatedWork = token is RawRepLoadToken { Sets: > 1, Loads.Count: 1 }
                    ? result.Items.OfType<WorkoutWorkSemantic>().SingleOrDefault()
                    : null;
                previousTokenEnd = token.Span.End;
                if (result.Warning is { } warning)
                {
                    var sourceText = prescriptionSpan.Read(input.Cell.Text);
                    warnings.Add(new WorkoutImportWarning
                    {
                        Severity = WorkoutImportIssueSeverity.Warning,
                        Message = $"{input.Cell.SourceReference}: '{sourceText}' — {warning}"
                    });
                }

                if (result.Items.Count > 0)
                {
                    input.Cell.Consume(prescriptionSpan, ConsumptionKind.Prescription);
                    semanticItems.AddRange(result.Items);
                }
            }
        }

        return warnings;
    }

    private IReadOnlyList<WorkoutImportExerciseInterpretation> CreateExerciseInterpretations(
        RowExercise? rowExercise,
        IReadOnlyList<WorkoutImportSemanticItem> semanticItems)
    {
        if (rowExercise is null)
        {
            return [];
        }

        var itemsByTarget = semanticItems.ToLookup(item => item.Origin.Target);
        var interpretations = new List<WorkoutImportExerciseInterpretation>();
        foreach (var fragment in rowExercise.Fragments)
        {
            foreach (var match in fragment.Matches)
            {
                var exerciseContext = CreatePrescriptionContext(fragment, match);
                if (exerciseContext is null)
                {
                    continue;
                }

                var matchText = match.Span.Read(fragment.Input.Text);
                var source = new WorkoutSemanticSource(
                    matchText,
                    exerciseContext.SourceReference,
                    exerciseContext.SourceOffset,
                    fragment.Input.Cell.ColumnIndex);
                var target = new WorkoutExerciseSemanticTarget(exerciseContext.Exercise.Id, source.Reference, source.Offset);
                var exerciseItems =
                    itemsByTarget[target]
                        .Distinct()
                        .ToList();
                interpretations.Add(new WorkoutImportExerciseInterpretation(exerciseContext.Exercise, source, exerciseItems));
            }
        }

        return interpretations;
    }

    private ExercisePrescriptionContext? FindFragmentExercise(
        ExerciseFragment fragment,
        TextSpan prescriptionSpan,
        RawPrescriptionToken token)
    {
        var tokenStart = prescriptionSpan.Start - fragment.Input.Span.Start;
        var tokenEnd = prescriptionSpan.End - fragment.Input.Span.Start;
        var followingMatch = fragment.Matches.FirstOrDefault(match => match.Span.Start >= tokenEnd);
        if (token is RawWorkToken { RequiresFollowingExercise: true })
        {
            if (followingMatch is null)
            {
                return null;
            }

            var gap = fragment.Input.Text[tokenEnd..followingMatch.Span.Start];
            return string.IsNullOrWhiteSpace(gap) ? CreatePrescriptionContext(fragment, followingMatch) : null;
        }

        var precedingMatch = fragment.Matches.LastOrDefault(match => match.Span.End <= tokenStart);
        if (token is RawCalorieRateToken && precedingMatch is not null)
        {
            var prefix = fragment.Input.Text[precedingMatch.Span.End..tokenStart].Trim();
            if (prefix is "" or "(")
            {
                return CreatePrescriptionContext(fragment, precedingMatch);
            }
        }

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

        var fragments =
            row.GetUnconsumedText()
                .SelectMany(SplitInput)
                .ToList();

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
