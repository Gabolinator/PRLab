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
    private readonly IReadOnlyList<Qualifier> equipmentAliases = BuildEquipmentAliases(profile);

    public ExerciseAttributes Interpret(string text, TextSpan exerciseSpan, int nextExerciseStart, bool isFirstMatch)
    {
        var assignments = new List<WorkoutImportAttributeAssignment>();
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
                var precedingMatches = leadingMatches.Where(match => match.Span.Start < exerciseSpan.Start);
                foreach (var match in precedingMatches)
                {
                    assignments.AddRange(match.Assignments);
                    var end = Math.Min(match.Span.End, exerciseSpan.Start);
                    spans.Add(new TextSpan(match.Span.Start, end - match.Span.Start));
                    matchedStart = Math.Min(matchedStart, match.Span.Start);
                }

                var overlappingMatches = leadingMatches.Where(match => match.Span.Start >= exerciseSpan.Start);
                foreach (var match in overlappingMatches)
                {
                    assignments.AddRange(match.Assignments);
                }
            }
        }

        var trailingMatches = MatchTrailingQualifiers(text, exerciseSpan.End, nextExerciseStart);
        foreach (var match in trailingMatches)
        {
            assignments.AddRange(match.Assignments);
            spans.Add(match.Span);
        }

        var exerciseText = exerciseSpan.Read(text);
        var matchedText = text[matchedStart..exerciseSpan.End];
        assignments.AddRange(GetAliasAssignments(matchedText));
        assignments.AddRange(GetAliasAssignments(exerciseText));
        var distinctAssignments = assignments.DistinctBy(attribute => (attribute.Type, attribute.Value)).ToList();
        var remainingStart = trailingMatches.Count == 0 ? exerciseSpan.End : trailingMatches[^1].Span.End;
        var remainingText = text[remainingStart..nextExerciseStart].Trim();

        return new ExerciseAttributes(matchedText, remainingText, distinctAssignments, spans);
    }

    public IEnumerable<(TextSpan Span, WorkoutImportAttributeAssignment Assignment)> FindEquipment(string text)
    {
        var matches = equipmentAliases.SelectMany(alias => alias.Pattern.Matches(text).Cast<Match>()
            .Select(match => (Match: match, Assignment: alias.Assignments[0])));
        var matchesInTextOrder = matches.OrderBy(entry => entry.Match.Index)
            .ThenByDescending(entry => entry.Match.Length);
        var end = 0;
        foreach (var (match, assignment) in matchesInTextOrder)
        {
            if (match.Index >= end)
            {
                var span = new TextSpan(match.Index, match.Length);
                yield return (span, assignment);
                end = span.End;
            }
        }
    }

    private static IReadOnlyList<Qualifier> BuildEquipmentAliases(WorkoutImportProfile profile) =>
        profile.Aliases.Equipment
            .Where(alias => !string.IsNullOrWhiteSpace(alias.Key) && !string.IsNullOrWhiteSpace(alias.Value))
            .Select(alias => new Qualifier(
                WorkoutTextMatching.CreatePhrasePattern(
                    alias.Key, WorkoutTextMatching.WordStart, WorkoutTextMatching.WordEnd),
                [new WorkoutImportAttributeAssignment { Type = WorkoutImportAttributeType.Equipment, Value = alias.Value }]))
            .ToList();

    private static IReadOnlyList<Qualifier> BuildQualifiers(
        WorkoutImportProfile profile, WorkoutImportAttributeMatchMode mode, string boundary) =>
        profile.Aliases.Attributes
            .Where(entry => entry.Value.MatchMode == mode && !string.IsNullOrWhiteSpace(entry.Key))
            .Select(entry => new Qualifier(
                WorkoutTextMatching.CreatePhrasePattern(entry.Key, "^", boundary), entry.Value.Assignments))
            .ToList();

    private static ILookup<string, WorkoutImportAttributeAssignment> BuildAliasLookup(WorkoutImportProfile profile)
    {
        var aliasRules = profile.Aliases.Attributes.Where(entry =>
            entry.Value.MatchMode == WorkoutImportAttributeMatchMode.MatchedAlias &&
            !string.IsNullOrWhiteSpace(entry.Key));
        var assignments = aliasRules.SelectMany(entry =>
        {
            var aliasKey = FormatingUtilities.NormalizeNonNullString(entry.Key);
            return entry.Value.Assignments.Select(attribute => (AliasKey: aliasKey, Attribute: attribute));
        });
        return assignments.ToLookup(entry => entry.AliasKey, entry => entry.Attribute);
    }

    private static (TextSpan Span, IReadOnlyList<WorkoutImportAttributeAssignment> Assignments)? FindQualifier(
        IReadOnlyList<Qualifier> qualifiers, string text, int offset)
    {
        var matches = qualifiers.Select(qualifier => (Qualifier: qualifier, Match: qualifier.Pattern.Match(text)));
        var bestMatch = matches
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

    private sealed record Qualifier(Regex Pattern, IReadOnlyList<WorkoutImportAttributeAssignment> Assignments);
}

internal sealed record ExerciseAttributes(
    string MatchedText,
    string RemainingText,
    IReadOnlyList<WorkoutImportAttributeAssignment> Assignments,
    IReadOnlyList<TextSpan> Spans);
