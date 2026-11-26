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
                var result = InterpretExercisePart(part, out var resolvedLength);
                TextSpan? resolvedSpan = null;
                if (result.ExerciseId is not null)
                {
                    var leadingWhitespaceLength = part.Length - part.TrimStart().Length;
                    var resolvedStart = offset + leadingWhitespaceLength;
                    resolvedSpan = new TextSpan(resolvedStart, resolvedLength);
                }

                results.Add((result, resolvedSpan));
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
        var pattern = CreatePrefixPattern(matchText, WordBoundary);
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

    private static Regex CreatePrefixPattern(string text, string boundary)
    {
        var normalizedText = FormatingUtilities.NormalizeNonNullString(text);
        var words = normalizedText.Split(' ');
        var escapedWords = words.Select(Regex.Escape);
        var pattern = string.Join(@"[^\S\r\n]+", escapedWords);
        return new Regex($"^{pattern}{boundary}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private WorkoutImportExercisePart InterpretExercisePart(string sourceText, out int resolvedLength)
    {
        resolvedLength = 0;
        var text = sourceText.TrimStart();
        var qualifierAttributes = new List<WorkoutImportAttributeAssignment>();
        var (candidate, exerciseOffset, matchEnd) = MatchExerciseWithLeadingQualifiers(text, qualifierAttributes);

        // if candidate is null we need to check if a combination of exercises

        if (candidate is null)
        {
            var unmatchedText = text.TrimEnd();
            return new WorkoutImportExercisePart
            {
                SourceText = sourceText,
                Attributes = qualifierAttributes,
                Warning = $"No catalog exercise matches '{unmatchedText}'."
            };
        }

        var matchedText = text[..matchEnd];
        var textAfterMatch = text[matchEnd..];
        var textAfterQualifiers = MatchTrailingQualifiers(textAfterMatch, qualifierAttributes);
        resolvedLength = text.Length - textAfterQualifiers.Length;
        var remainingText = textAfterQualifiers.Trim();
        var exerciseText = text[exerciseOffset..matchEnd];
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
        var (bestCandidate, matchLength) = FindExercise(text);
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
                var (candidate, length) = FindExercise(remainder);
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

    private (ExerciseCandidate? Candidate, int Length) FindExercise(string text)
    {
        var match = candidates
            .Select(candidate => (Candidate: candidate, Length: candidate.Pattern.Match(text).Length))
            .DefaultIfEmpty()
            .MaxBy(match => match.Length);
        return match.Length > 0 ? match : default;
    }

    private sealed record ExerciseCandidate(
        Regex Pattern,
        WorkoutImportExerciseReference? Exercise,
        bool Ambiguous);

    private sealed record Qualifier(Regex Pattern, IReadOnlyList<WorkoutImportAttributeAssignment> Attributes);
}
