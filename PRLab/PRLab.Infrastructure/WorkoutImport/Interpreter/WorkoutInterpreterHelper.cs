using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutInterpreterHelper
{
    private readonly string[] separators;
    private readonly Regex[] ignoredLabels;
    private readonly IReadOnlyList<ExerciseCandidate> candidates;
    private readonly IReadOnlyList<Qualifier> leadingQualifiers;
    private readonly IReadOnlyList<Qualifier> trailingQualifiers;
    private readonly ILookup<string, WorkoutImportAttributeAssignment> aliasAttributes;
    private const string WordStart = @"(?<![\p{L}\p{M}\p{N}_])";
    private const string WordBoundary = @"(?![\p{L}\p{M}\p{N}_])";

    public WorkoutInterpreterHelper(
        WorkoutImportProfile profile,
        IReadOnlyList<WorkoutImportExerciseReference> exercises)
    {
        separators = profile.Parsing.Separators.Where(separator => !string.IsNullOrEmpty(separator)).ToArray();
        ignoredLabels = BuildIgnoredLabelPatterns(profile);
        candidates = BuildExerciseCandidates(profile, exercises);
        leadingQualifiers = BuildQualifiers(profile, WorkoutImportAttributeMatchMode.LeadingQualifier, @"(?=[^\S\r\n]|$)");
        trailingQualifiers = BuildQualifiers(profile, WorkoutImportAttributeMatchMode.TrailingQualifier, WordBoundary);
        aliasAttributes = BuildMatchedAliasAttributeLookup(profile);
    }

    public bool IsIgnored(string text)
    {
        var trimmedText = text.TrimStart();
        return ignoredLabels.Any(pattern => pattern.IsMatch(trimmedText));
    }

    // Resolved spans are relative to the supplied text and include any matched qualifiers.
    public IReadOnlyList<(WorkoutImportExercisePart Part, TextSpan? ResolvedSpan)> InterpretExerciseText(string text)
    {
        string[] parts = separators.Length == 0
            ? [text]
            : text.Split(separators, StringSplitOptions.None);

        var results = new List<(WorkoutImportExercisePart, TextSpan?)>();
        var offset = 0;
        foreach (var part in parts)
        {
            if (!string.IsNullOrWhiteSpace(part) && !IsIgnored(part))
            {
                var partResults = InterpretExercisePart(part, offset);
                results.AddRange(partResults);
            }

            offset += part.Length;
            if (offset < text.Length)
            {
                var matchedSeparator = separators.First(separator =>
                    text.AsSpan(offset).StartsWith(separator.AsSpan(), StringComparison.Ordinal));
                offset += matchedSeparator.Length;
            }
        }

        return results;
    }

    private static Regex[] BuildIgnoredLabelPatterns(WorkoutImportProfile profile) =>
        profile.Parsing.IgnoredLabels
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => CreatePrefixPattern(label, @"(?=[^\S\r\n]*(?:$|\r|\n))"))
            .ToArray();

    private static IReadOnlyList<ExerciseCandidate> BuildExerciseCandidates(
        WorkoutImportProfile profile,
        IReadOnlyList<WorkoutImportExerciseReference> exercises)
    {
        var exercisesByKey = exercises.ToLookup(exercise => exercise.NameKey);
        var names = exercises.Select(exercise => (Text: exercise.Name, Key: exercise.NameKey));
        var aliases = profile.Aliases.Exercises
            .SelectMany(entry => entry.Value.Select(alias =>
                (Text: alias, Key: FormatingUtilities.NormalizeNameKey(entry.Key))));

        var namesAndAliases = names.Concat(aliases);
        var entriesByMatchText = namesAndAliases
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Text))
            .GroupBy(entry => FormatingUtilities.NormalizeNonNullString(entry.Text));

        return entriesByMatchText
            .Select(group =>
            {
                var targetKeys = group.Select(entry => entry.Key);
                return CreateExerciseCandidate(group.Key, targetKeys, exercisesByKey);
            })
            .ToList();
    }

    private static ExerciseCandidate CreateExerciseCandidate(
        string matchText,
        IEnumerable<string> targetKeys,
        ILookup<string, WorkoutImportExerciseReference> exercisesByKey)
    {
        var keys = targetKeys.Distinct().ToList();
        var matches = keys.SelectMany(key => exercisesByKey[key]).ToList();
        var ambiguous = keys.Count > 1 || matches.Count > 1;
        var pattern = CreateTextPattern(matchText, WordStart, WordBoundary);
        var exercise = ambiguous ? null : matches.SingleOrDefault();
        return new ExerciseCandidate(pattern, exercise, ambiguous);
    }

    private static ILookup<string, WorkoutImportAttributeAssignment> BuildMatchedAliasAttributeLookup(
        WorkoutImportProfile profile) =>
        profile.Aliases.Attributes
            .Where(entry => entry.Value.MatchMode == WorkoutImportAttributeMatchMode.MatchedAlias
                && !string.IsNullOrWhiteSpace(entry.Key))
            .SelectMany(entry => entry.Value.Assignments.Select(attribute =>
                (Key: FormatingUtilities.NormalizeNonNullString(entry.Key), Attribute: attribute)))
            .ToLookup(entry => entry.Key, entry => entry.Attribute);

    private static IReadOnlyList<Qualifier> BuildQualifiers(
        WorkoutImportProfile profile, WorkoutImportAttributeMatchMode mode, string boundary) =>
        profile.Aliases.Attributes
            .Where(entry => entry.Value.MatchMode == mode && !string.IsNullOrWhiteSpace(entry.Key))
            .Select(entry => new Qualifier(CreatePrefixPattern(entry.Key, boundary), entry.Value.Assignments))
            .ToList();

    private static string? GetExerciseMatchWarning(ExerciseCandidate candidate, string matchedText) =>
        candidate.Ambiguous
            ? $"Exercise name or alias '{matchedText}' is ambiguous."
            : candidate.Exercise is null
                ? $"Exercise alias '{matchedText}' points to a missing catalog exercise."
                : null;

    private static (Qualifier? Qualifier, int Length) FindQualifier(
        IReadOnlyList<Qualifier> qualifiers, string text) =>
        qualifiers
            .Select(qualifier => (Qualifier: qualifier, Length: qualifier.Pattern.Match(text).Length))
            .DefaultIfEmpty()
            .MaxBy(match => match.Length);

    private static Regex CreatePrefixPattern(string text, string boundary) =>
        CreateTextPattern(text, "^", boundary);

    private static Regex CreateTextPattern(string text, string start, string end)
    {
        var normalizedText = FormatingUtilities.NormalizeNonNullString(text);
        var words = normalizedText.Split(' ');
        var escapedWords = words.Select(Regex.Escape);
        var pattern = string.Join(@"[^\S\r\n]+", escapedWords);
        return new Regex($"{start}{pattern}{end}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private IEnumerable<(WorkoutImportExercisePart Part, TextSpan? ResolvedSpan)> InterpretExercisePart(
        string sourceText, int partOffset)
    {
        var text = sourceText.TrimStart();
        var textOffset = partOffset + sourceText.Length - text.Length;
        var qualifierAttributes = new List<WorkoutImportAttributeAssignment>();
        var (candidate, exerciseOffset, matchEnd) = MatchExerciseWithLeadingQualifiers(text, qualifierAttributes);
        var matchStart = 0;

        if (candidate is null)
        {
            (candidate, exerciseOffset, matchEnd) = FindNextExercise(text, 0);
            if (candidate is null)
            {
                var unmatchedText = text.TrimEnd();
                var unresolvedPart = new WorkoutImportExercisePart
                {
                    SourceText = sourceText,
                    Attributes = qualifierAttributes,
                    Warning = $"No catalog exercise matches '{unmatchedText}'."
                };
                yield return (unresolvedPart, null);
                yield break;
            }

            matchStart = exerciseOffset;
            qualifierAttributes.Clear();
        }

        while (candidate is not null)
        {
            var matchedText = text[matchStart..matchEnd];
            var exerciseText = text[exerciseOffset..matchEnd];
            var textAfterMatch = text[matchEnd..];
            var textAfterQualifiers = MatchTrailingQualifiers(textAfterMatch, qualifierAttributes);
            var resolvedEnd = text.Length - textAfterQualifiers.Length;
            var (nextCandidate, nextStart, nextEnd) = FindNextExercise(text, resolvedEnd);
            var remainingEnd = nextCandidate is null ? text.Length : nextStart;
            var remainingText = text[resolvedEnd..remainingEnd].Trim();

            var part = CreateMatchedExercisePart(
                sourceText, candidate, matchedText, exerciseText, remainingText, qualifierAttributes);
            TextSpan? resolvedSpan = part.ExerciseId is null
                ? null
                : new TextSpan(textOffset + matchStart, resolvedEnd - matchStart);
            yield return (part, resolvedSpan);

            candidate = nextCandidate;
            matchStart = nextStart;
            exerciseOffset = nextStart;
            matchEnd = nextEnd;
            qualifierAttributes.Clear();
        }
    }

    private WorkoutImportExercisePart CreateMatchedExercisePart(
        string sourceText,
        ExerciseCandidate candidate,
        string matchedText,
        string exerciseText,
        string remainingText,
        List<WorkoutImportAttributeAssignment> qualifierAttributes)
    {
        var attributes = CollectMatchedAttributes(qualifierAttributes, matchedText, exerciseText);
        var exercise = candidate.Exercise;
        var warning = GetExerciseMatchWarning(candidate, matchedText);

        return new WorkoutImportExercisePart
        {
            SourceText = sourceText,
            MatchedText = matchedText,
            RemainingText = remainingText.Length == 0 ? null : remainingText,
            ExerciseName = exercise?.Name,
            ExerciseId = exercise?.Id,
            Attributes = attributes,
            Warning = warning
        };
    }

    private (ExerciseCandidate? Candidate, int ExerciseOffset, int MatchEnd) MatchExerciseWithLeadingQualifiers(
        string text, List<WorkoutImportAttributeAssignment> qualifierAttributes)
    {
        var (bestCandidate, matchLength) = FindExerciseAtStart(text);
        var exerciseOffset = 0;
        var qualifierLimit = bestCandidate is null ? text.Length : matchLength;

        while (exerciseOffset < qualifierLimit)
        {
            var prefix = text[exerciseOffset..qualifierLimit];
            var qualifierMatch = FindQualifier(leadingQualifiers, prefix);

            if (qualifierMatch.Length == 0)
            {
                break;
            }

            qualifierAttributes.AddRange(qualifierMatch.Qualifier!.Attributes);
            var textAfterQualifier = prefix[qualifierMatch.Length..];
            var remainder = textAfterQualifier.TrimStart();
            exerciseOffset = qualifierLimit - remainder.Length;

            if (bestCandidate is null)
            {
                var (candidate, length) = FindExerciseAtStart(remainder);
                if (candidate is not null)
                {
                    var matchEnd = exerciseOffset + length;
                    return (candidate, exerciseOffset, matchEnd);
                }
            }
        }

        return (bestCandidate, exerciseOffset, matchLength);
    }

    private string MatchTrailingQualifiers(
        string remainingText, List<WorkoutImportAttributeAssignment> qualifierAttributes)
    {
        while (remainingText.Length > 0 &&
               char.IsWhiteSpace(remainingText[0]) &&
               remainingText[0] is not ('\r' or '\n'))
        {
            var suffix = remainingText.TrimStart(' ', '\t');
            var qualifierMatch = FindQualifier(trailingQualifiers, suffix);
            if (qualifierMatch.Length == 0)
            {
                break;
            }

            qualifierAttributes.AddRange(qualifierMatch.Qualifier!.Attributes);
            remainingText = suffix[qualifierMatch.Length..];
        }

        return remainingText;
    }

    private IReadOnlyList<WorkoutImportAttributeAssignment> CollectMatchedAttributes(
        List<WorkoutImportAttributeAssignment> qualifierAttributes, string matchedText, string exerciseText)
    {
        var matchedTextKey = FormatingUtilities.NormalizeNonNullString(matchedText);
        var exerciseTextKey = FormatingUtilities.NormalizeNullableString(exerciseText);
        var matchedTextAttributes = aliasAttributes[matchedTextKey];
        var exerciseTextAttributes = aliasAttributes[exerciseTextKey];

        return qualifierAttributes
            .Concat(matchedTextAttributes)
            .Concat(exerciseTextAttributes)
            .DistinctBy(attribute => (attribute.Type, attribute.Value))
            .ToList();
    }

    private (ExerciseCandidate? Candidate, int Length) FindExerciseAtStart(string text)
    {
        var (candidate, start, end) = FindNextExercise(text, 0);
        return start == 0 ? (candidate, end) : default;
    }

    private (ExerciseCandidate? Candidate, int Start, int End) FindNextExercise(string text, int offset)
    {
        ExerciseCandidate? bestCandidate = null;
        var bestStart = text.Length;
        var bestLength = 0;

        foreach (var candidate in candidates)
        {
            var match = candidate.Pattern.Match(text, offset);
            if (!match.Success)
            {
                continue;
            }

            var startsEarlier = match.Index < bestStart;
            var isLongerAtSameStart = match.Index == bestStart && match.Length > bestLength;
            if (startsEarlier || isLongerAtSameStart)
            {
                bestCandidate = candidate;
                bestStart = match.Index;
                bestLength = match.Length;
            }
        }

        return (bestCandidate, bestStart, bestStart + bestLength);
    }

    private sealed record ExerciseCandidate(
        Regex Pattern,
        WorkoutImportExerciseReference? Exercise,
        bool Ambiguous);

    private sealed record Qualifier(Regex Pattern, IReadOnlyList<WorkoutImportAttributeAssignment> Attributes);
}
