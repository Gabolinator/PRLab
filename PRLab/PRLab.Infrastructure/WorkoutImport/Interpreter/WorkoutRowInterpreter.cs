using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.WorkoutImport.Interpreter.Extensions;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutRowInterpreter(WorkoutImportContext context, IAppLogger logger)
{
    private readonly ExerciseInterpreter exercises = new(context.Profile, context.Exercises);

    private readonly AttributeInterpreter attributes = new(context.Profile);

    private readonly WorkoutSetupRecognizer setup = new(context.Profile);

    private readonly WorkoutEquipmentMatcher equipmentMatcher = new(context);

    private readonly IReadOnlyDictionary<EquipmentId, WorkoutImportEquipmentReference> equipmentById =
        context.Equipment.ToDictionary(equipment => equipment.Id);

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

    private readonly ILookup<string, string> profileAliasTargets =
        context.Profile.Aliases.Exercises
            .SelectMany(entry => entry.Value
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Select(alias => (Phrase: FormatingUtilities.NormalizeNonNullString(alias),
                    Target: FormatingUtilities.NormalizeNameKey(entry.Key))))
            .ToLookup(entry => entry.Phrase, entry => entry.Target);

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
        var semanticItems = new List<WorkoutImportSemanticItem>();
        var blockHeading = RecognizeBlockHeading(expectedExerciseCell);
        semanticItems.AddRange(blockHeading);

        var notesCell = row.GetExpectedCell(WorkoutImportProperty.Notes);
        var equipmentInputs =
            row.GetUnconsumedText()
                .Where(input => input.Cell != notesCell)
                .SelectMany(SplitInput)
                .ToList();
        var exercise = FindRowExercise(row, expectedExerciseCell);
        var detailsCell = row.GetExpectedCell(WorkoutImportProperty.Details);
        var exerciseParts = exercise is null ? [] : RecognizeExerciseQualifiers(exercise.Fragments, detailsCell, semanticItems);
        var workWindows = RecognizeWorkWindows(row);
        semanticItems.AddRange(workWindows);

        var warnings = new List<WorkoutImportWarning>();
        var exerciseTargets = GetExerciseTargets(exercise);
        RecognizeEquipment(equipmentInputs, exercise?.Input.Cell, detailsCell, exerciseTargets, semanticItems, warnings);
        RecognizeEquipmentHeights(equipmentInputs, exercise?.Input.Cell, detailsCell, exerciseTargets, semanticItems, warnings);
        if (semanticItems.Count == 0 && exercise is null && warnings.Count == 0)
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

        warnings.AddRange(RecognizePrescriptions(row, exercise, semanticItems));
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
        var rowSource = semanticItems.FirstOrDefault()?.Origin.Source;
        var sourceText = exercise?.Input.Text ?? workWindows.FirstOrDefault()?.Origin.Source.Text ??
            rowSource?.Text ?? equipmentInputs.FirstOrDefault()?.Text ?? string.Empty;
        var sourceReference = exercise?.Input.Cell.SourceReference ??
            workWindows.FirstOrDefault()?.Origin.Source.Reference ?? rowSource?.Reference ??
            equipmentInputs.FirstOrDefault()?.Cell.SourceReference;

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

    private IReadOnlyList<WorkoutImportSemanticItem> RecognizeBlockHeading(WorkoutRowCell? cell)
    {
        if (cell is null || cell.Consumption.Count > 0 || string.IsNullOrWhiteSpace(cell.Text))
        {
            return [];
        }

        var source = new WorkoutSemanticSource(cell.Text, cell.SourceReference, 0, cell.ColumnIndex);
        var items = setup.RecognizeBlockHeading(source);
        foreach (var item in items)
        {
            var matched = item.Origin.Source;
            cell.Consume(new TextSpan(matched.Offset, matched.Length), ConsumptionKind.Attribute);
        }

        return items;
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

                semanticItems.AddRange(result.Items.Select(ResolveConfiguredEquipment));

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
                    .Concat(setup.FindExerciseCues(source, target))
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
                items.Add(ResolveConfiguredEquipment(item));
                previousEnd = matchedSpan.End;
            }
        }

        return items;
    }

    private WorkoutImportSemanticItem ResolveConfiguredEquipment(WorkoutImportSemanticItem item) =>
        item is WorkoutEquipmentSemantic equipment ? equipmentMatcher.ResolveConfiguredName(equipment) : item;

    private void RecognizeEquipment(
        IReadOnlyList<UnconsumedCellText> inputs,
        WorkoutRowCell? exerciseCell,
        WorkoutRowCell? detailsCell,
        IReadOnlyList<WorkoutExerciseSemanticTarget> exerciseTargets,
        List<WorkoutImportSemanticItem> items,
        List<WorkoutImportWarning> warnings)
    {
        foreach (var input in inputs)
        {
            var source = CreateSemanticSource(input);
            foreach (var match in equipmentMatcher.FindMatches(source))
            {
                var span = new TextSpan(match.Source.Offset, match.Source.Length);
                var overlappingConsumption =
                    input.Cell.Consumption
                        .Where(entry => entry.Span.Start < span.End && entry.Span.End > span.Start)
                        .ToList();
                var coversExercise = overlappingConsumption.Count == 1 &&
                    overlappingConsumption[0] is { Kind: ConsumptionKind.Exercise } consumed &&
                    consumed.Span == match.LabelSpan;
                if (overlappingConsumption.Count > 0 && !coversExercise)
                {
                    continue;
                }

                if (match.Warning is { } warning)
                {
                    warnings.Add(new WorkoutImportWarning
                    {
                        Severity = WorkoutImportIssueSeverity.Warning,
                        Message = $"{source.Reference}: {warning}"
                    });
                }

                if (match.Equipment is not { } equipment)
                {
                    continue;
                }

                var target = FindEquipmentTarget(input, match.LabelSpan, exerciseCell, detailsCell, exerciseTargets);
                if (target is not null)
                {
                    equipment = equipment with { Origin = equipment.Origin with { Target = target } };
                }

                if (coversExercise)
                {
                    var prefixLength = match.LabelSpan.Start - span.Start;
                    if (prefixLength > 0)
                    {
                        input.Cell.Consume(new TextSpan(span.Start, prefixLength), ConsumptionKind.Equipment);
                    }
                }
                else
                {
                    input.Cell.Consume(span, ConsumptionKind.Equipment);
                }

                items.Add(equipment);
            }
        }
    }

    private static IReadOnlyList<WorkoutExerciseSemanticTarget> GetExerciseTargets(RowExercise? exercise) =>
        (exercise?.Fragments ?? [])
            .SelectMany(fragment => fragment.Matches
                .Where(match => match.Part.ExerciseId.HasValue)
                .Select(match => new WorkoutExerciseSemanticTarget(
                    match.Part.ExerciseId!.Value,
                    fragment.Input.Cell.SourceReference,
                    fragment.Input.Span.Start + match.Span.Start)))
            .ToList();

    private void RecognizeEquipmentHeights(
        IReadOnlyList<UnconsumedCellText> inputs,
        WorkoutRowCell? exerciseCell,
        WorkoutRowCell? detailsCell,
        IReadOnlyList<WorkoutExerciseSemanticTarget> exerciseTargets,
        List<WorkoutImportSemanticItem> items,
        List<WorkoutImportWarning> warnings)
    {
        var explicitEquipment =
            items
                .OfType<WorkoutEquipmentSemantic>()
                .Where(item => item.EquipmentId.HasValue)
                .ToList();

        foreach (var input in inputs)
        {
            var source = CreateSemanticSource(input);
            foreach (var match in WorkoutSetupRecognizer.FindEquipmentHeights(source))
            {
                var span = new TextSpan(match.Source.Offset, match.Source.Length);
                if (input.Cell.Consumption.Any(entry => entry.Span.Start < span.End && entry.Span.End > span.Start))
                {
                    continue;
                }

                var exerciseTarget = FindEquipmentTarget(input, span, exerciseCell, detailsCell, exerciseTargets);
                var adjacentEquipment =
                    explicitEquipment
                        .Where(item => item.Origin.Source.ColumnIndex == input.Cell.ColumnIndex &&
                            IsAdjacent(input.Cell.Text, span, item.Origin.Source))
                        .ToList();
                var adjacentTargets =
                    adjacentEquipment
                        .Select(item => item.Origin.Target)
                        .OfType<WorkoutExerciseSemanticTarget>()
                        .Distinct()
                        .ToList();
                var target = exerciseTarget ?? (adjacentTargets.Count == 1 ? adjacentTargets[0] : null);
                var availableIds = adjacentEquipment.Count > 0
                    ? adjacentEquipment.Select(item => item.EquipmentId!.Value)
                    : GetExerciseEquipmentIds(target, explicitEquipment);
                var availableEquipment =
                    availableIds
                        .Distinct()
                        .Select(id => equipmentById.GetValueOrDefault(id))
                        .OfType<WorkoutImportEquipmentReference>()
                        .ToList();
                var (equipment, warning) = WorkoutSetupRecognizer.RecognizeEquipmentHeight(match, target, availableEquipment);
                if (warning is not null)
                {
                    warnings.Add(new WorkoutImportWarning
                    {
                        Severity = WorkoutImportIssueSeverity.Warning,
                        Message = $"{source.Reference}: {warning}"
                    });
                }

                if (equipment is not null)
                {
                    items.Add(equipment);
                    input.Cell.Consume(span, ConsumptionKind.Equipment);
                }
            }
        }
    }

    private IEnumerable<EquipmentId> GetExerciseEquipmentIds(
        WorkoutExerciseSemanticTarget? target, IReadOnlyList<WorkoutEquipmentSemantic> explicitEquipment)
    {
        if (target is null)
        {
            return [];
        }

        var exercise = context.Exercises.FirstOrDefault(reference => reference.Id == target.ExerciseId);
        var explicitIds =
            explicitEquipment
                .Where(item => item.Origin.Target == target)
                .Select(item => item.EquipmentId!.Value);

        return (exercise?.EquipmentIds ?? []).Concat(explicitIds);
    }

    private static bool IsAdjacent(string text, TextSpan measurement, WorkoutSemanticSource equipment)
    {
        var equipmentEnd = equipment.Offset + equipment.Length;
        if (equipmentEnd <= measurement.Start)
        {
            return string.IsNullOrWhiteSpace(text[equipmentEnd..measurement.Start]);
        }

        return measurement.End <= equipment.Offset &&
            string.IsNullOrWhiteSpace(text[measurement.End..equipment.Offset]);
    }

    private static WorkoutExerciseSemanticTarget? FindEquipmentTarget(
        UnconsumedCellText input,
        TextSpan labelSpan,
        WorkoutRowCell? exerciseCell,
        WorkoutRowCell? detailsCell,
        IReadOnlyList<WorkoutExerciseSemanticTarget> targets)
    {
        if (input.Cell == exerciseCell)
        {
            var fragmentTargets =
                targets
                    .Where(target => target.SourceOffset >= input.Span.Start && target.SourceOffset < input.Span.End)
                    .ToList();
            var exactTarget = fragmentTargets.FirstOrDefault(target => target.SourceOffset == labelSpan.Start);
            return exactTarget ?? (fragmentTargets.Count == 1 ? fragmentTargets[0] : null);
        }

        return input.Cell == detailsCell && targets.Count == 1 ? targets[0] : null;
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
            var tokens = WorkoutPrescriptionSyntax.Scan(inputText, context.Profile.Parsing).ToList();
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
                var seenQualities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var exerciseItems =
                    itemsByTarget[target]
                        .Where(item => item is not WorkoutMovementQualitySemantic quality || seenQualities.Add(quality.Quality))
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
        var profileTargets = profileAliasTargets[hintKey];
        var profileHintApplies = profileTargets.All(target => target == exercise.NameKey);
        WorkTargetType? hint = profileHintApplies && prescriptionHints.TryGetValue(hintKey, out var configuredHint)
            ? configuredHint
            : null;
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
