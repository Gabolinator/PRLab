using System.Globalization;
using PRLab.Application.Models.ImportDraft;
using System.Text.RegularExpressions;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class AttributeInterpreter(WorkoutImportProfile profile)
{
    private readonly IReadOnlyList<Qualifier> leading = BuildQualifiers(
        profile, WorkoutImportAttributeMatchMode.LeadingQualifier, @"(?=[^\S\r\n]|$)");
    private readonly IReadOnlyList<Qualifier> trailing = BuildQualifiers(
        profile, WorkoutImportAttributeMatchMode.TrailingQualifier, WorkoutTextMatching.WordEnd);
    private readonly ILookup<string, WorkoutImportAttributeAssignment> matchedAliases = BuildAliasLookup(profile);
    private readonly IReadOnlyList<EquipmentAlias> equipmentAliases = BuildEquipmentAliases(profile);

    public ExerciseQualifiers Recognize(
        WorkoutSemanticSource source, WorkoutExerciseSemanticTarget target,
        TextSpan exerciseSpan, int nextExerciseStart, bool isFirstMatch)
    {
        var text = source.Text;
        var items = new List<WorkoutImportSemanticItem>();
        var spans = new List<TextSpan>();
        var matchedStart = exerciseSpan.Start;

        if (isFirstMatch)
        {
            var leadingText = text[..exerciseSpan.End];
            var leadingMatches = MatchLeadingQualifiers(leadingText, exerciseSpan.Start);
            var leadingEnd = leadingMatches.LastOrDefault().Span.End;
            var gap = leadingEnd < exerciseSpan.Start ? text[leadingEnd..exerciseSpan.Start] : string.Empty;
            if (string.IsNullOrWhiteSpace(gap))
            {
                foreach (var match in leadingMatches)
                {
                    var recognized = RecognizeAssignments(match.Assignments, source, target, match.Span);
                    items.AddRange(recognized);
                    if (recognized.Count == 0 || match.Span.Start >= exerciseSpan.Start)
                    {
                        continue;
                    }

                    var end = Math.Min(match.Span.End, exerciseSpan.Start);
                    spans.Add(new TextSpan(match.Span.Start, end - match.Span.Start));
                    matchedStart = Math.Min(matchedStart, match.Span.Start);
                }
            }
        }

        var remainingStart = exerciseSpan.End;
        var trailingMatches = MatchTrailingQualifiers(text, exerciseSpan.End, nextExerciseStart);
        foreach (var match in trailingMatches)
        {
            var recognized = RecognizeAssignments(match.Assignments, source, target, match.Span);
            if (recognized.Count == 0)
            {
                break;
            }

            items.AddRange(recognized);
            spans.Add(match.Span);
            remainingStart = match.Span.End;
        }

        var matchedSpan = new TextSpan(matchedStart, exerciseSpan.End - matchedStart);
        var matchedText = matchedSpan.Read(text);
        var exerciseText = exerciseSpan.Read(text);
        items.AddRange(RecognizeAssignments(GetAliasAssignments(matchedText), source, target, matchedSpan));
        if (matchedSpan != exerciseSpan)
        {
            items.AddRange(RecognizeAssignments(GetAliasAssignments(exerciseText), source, target, exerciseSpan));
        }

        var remainingText = text[remainingStart..nextExerciseStart].Trim();
        return new ExerciseQualifiers(matchedText, remainingText, items, spans);
    }

    public IEnumerable<WorkoutEquipmentSemantic> FindEquipment(WorkoutSemanticSource source, WorkoutExerciseSemanticTarget target)
    {
        var matches =
            equipmentAliases
                .SelectMany(alias => alias.Pattern.Matches(source.Text)
                    .Cast<Match>()
                    .Select(match => (Match: match, Name: alias.Name)));
        var matchesInTextOrder =
            matches
                .OrderBy(entry => entry.Match.Index)
                .ThenByDescending(entry => entry.Match.Length);

        var end = 0;
        foreach (var (match, name) in matchesInTextOrder)
        {
            if (match.Index >= end)
            {
                var span = new TextSpan(match.Index, match.Length);
                var origin = CreateOrigin(source, target, span);
                yield return new WorkoutEquipmentSemantic(origin, name);
                end = span.End;
            }
        }
    }

    private static List<WorkoutImportSemanticItem> RecognizeAssignments(
        IEnumerable<WorkoutImportAttributeAssignment> assignments,
        WorkoutSemanticSource source,
        WorkoutExerciseSemanticTarget target,
        TextSpan span)
    {
        var origin = CreateOrigin(source, target, span);
        var items = new List<WorkoutImportSemanticItem>();
        foreach (var assignment in assignments)
        {
            var value = assignment.Value;
            WorkoutImportSemanticItem? item = assignment.Type switch
            {
                WorkoutImportAttributeType.Technique => new WorkoutTechniqueSemantic(origin, value),
                WorkoutImportAttributeType.MovementQuality => new WorkoutMovementQualitySemantic(origin, value),
                WorkoutImportAttributeType.Equipment => new WorkoutEquipmentSemantic(origin, value),
                WorkoutImportAttributeType.EquipmentQuantity when int.TryParse(value, out var quantity) =>
                    new WorkoutEquipmentQuantitySemantic(origin, quantity),
                WorkoutImportAttributeType.EquipmentHeight when decimal.TryParse(
                    value,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var height) =>
                    new WorkoutEquipmentHeightSemantic(origin, height),
                WorkoutImportAttributeType.EquipmentUnit => new WorkoutEquipmentUnitSemantic(origin, value),
                WorkoutImportAttributeType.AddedLoad when bool.TryParse(value, out var addedLoad) =>
                    new WorkoutAddedLoadSemantic(origin, addedLoad),
                WorkoutImportAttributeType.DistanceUnit => new WorkoutDistanceUnitSemantic(origin, value),
                _ => null
            };
            if (item is not null)
            {
                items.Add(item);
            }
        }

        return items;
    }

    private static WorkoutSemanticOrigin CreateOrigin(
        WorkoutSemanticSource source,
        WorkoutExerciseSemanticTarget target,
        TextSpan span)
    {
        var matchedSource = source with { Text = span.Read(source.Text), Offset = source.Offset + span.Start };
        return new WorkoutSemanticOrigin(matchedSource, target);
    }

    private static IReadOnlyList<EquipmentAlias> BuildEquipmentAliases(WorkoutImportProfile profile) =>
        profile.Aliases.Equipment
            .Where(alias => !string.IsNullOrWhiteSpace(alias.Key) && !string.IsNullOrWhiteSpace(alias.Value))
            .Select(alias => new EquipmentAlias(
                WorkoutTextMatching.CreatePhrasePattern(
                    alias.Key,
                    WorkoutTextMatching.WordStart,
                    WorkoutTextMatching.WordEnd),
                alias.Value))
            .ToList();

    private static IReadOnlyList<Qualifier> BuildQualifiers(
        WorkoutImportProfile profile,
        WorkoutImportAttributeMatchMode mode,
        string boundary) =>
        profile.Aliases.Attributes
            .Where(entry => entry.Value.MatchMode == mode && !string.IsNullOrWhiteSpace(entry.Key))
            .Select(entry => new Qualifier(
                WorkoutTextMatching.CreatePhrasePattern(entry.Key, "^", boundary),
                entry.Value.Assignments))
            .ToList();

    private static ILookup<string, WorkoutImportAttributeAssignment> BuildAliasLookup(WorkoutImportProfile profile)
    {
        var aliasRules =
            profile.Aliases.Attributes
                .Where(entry =>
                    entry.Value.MatchMode == WorkoutImportAttributeMatchMode.MatchedAlias &&
                    !string.IsNullOrWhiteSpace(entry.Key));
        var assignments =
            aliasRules
                .SelectMany(entry =>
                {
                    var aliasKey = FormatingUtilities.NormalizeNonNullString(entry.Key);
                    return entry.Value.Assignments.Select(attribute => (AliasKey: aliasKey, Attribute: attribute));
                });

        return assignments.ToLookup(entry => entry.AliasKey, entry => entry.Attribute);
    }

    private static (TextSpan Span, IReadOnlyList<WorkoutImportAttributeAssignment> Assignments)? FindQualifier(
        IReadOnlyList<Qualifier> qualifiers,
        string text,
        int offset)
    {
        var matches = qualifiers.Select(qualifier => (Qualifier: qualifier, Match: qualifier.Pattern.Match(text)));
        var bestMatch =
            matches
                .Where(entry => entry.Match.Success)
                .DefaultIfEmpty()
                .MaxBy(entry => entry.Match?.Length ?? 0);

        if (bestMatch.Match is null || bestMatch.Qualifier is null)
        {
            return null;
        }

        var span = new TextSpan(offset, bestMatch.Match.Length);
        return (span, bestMatch.Qualifier.Assignments);
    }

    private List<(TextSpan Span, IReadOnlyList<WorkoutImportAttributeAssignment> Assignments)> MatchLeadingQualifiers(
        string text, int exerciseStart)
    {
        var qualifiers = new List<(TextSpan, IReadOnlyList<WorkoutImportAttributeAssignment>)>();
        var offset = 0;
        while (offset < text.Length && offset <= exerciseStart)
        {
            var suffix = text[offset..];
            var trimmedSuffix = suffix.TrimStart();
            offset += suffix.Length - trimmedSuffix.Length;
            if (offset > exerciseStart || FindQualifier(leading, trimmedSuffix, offset) is not { } match)
            {
                break;
            }

            qualifiers.Add(match);
            offset = match.Span.End;
        }

        return qualifiers;
    }

    private List<(TextSpan Span, IReadOnlyList<WorkoutImportAttributeAssignment> Assignments)> MatchTrailingQualifiers(
        string text, int offset, int limit)
    {
        var qualifiers = new List<(TextSpan, IReadOnlyList<WorkoutImportAttributeAssignment>)>();
        while (offset < limit && text[offset] is ' ' or '\t')
        {
            var suffix = text[offset..limit];
            var trimmedSuffix = suffix.TrimStart(' ', '\t');
            var qualifierStart = offset + suffix.Length - trimmedSuffix.Length;
            if (FindQualifier(trailing, trimmedSuffix, qualifierStart) is not { } match)
            {
                break;
            }

            qualifiers.Add(match);
            offset = match.Span.End;
        }

        return qualifiers;
    }

    private IEnumerable<WorkoutImportAttributeAssignment> GetAliasAssignments(string text)
    {
        var aliasKey = FormatingUtilities.NormalizeNonNullString(text);
        return matchedAliases[aliasKey];
    }

    private sealed record EquipmentAlias(Regex Pattern, string Name);

    private sealed record Qualifier(Regex Pattern, IReadOnlyList<WorkoutImportAttributeAssignment> Assignments);
}

internal sealed record ExerciseQualifiers(
    string MatchedText,
    string RemainingText,
    IReadOnlyList<WorkoutImportSemanticItem> Items,
    IReadOnlyList<TextSpan> Spans);
