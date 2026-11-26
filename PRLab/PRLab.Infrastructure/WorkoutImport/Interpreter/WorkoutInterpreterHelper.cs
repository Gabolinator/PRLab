using System.Globalization;
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

        return names.Concat(aliases)
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Text))
            .GroupBy(entry => FormatingUtilities.NormalizeNonNullString(entry.Text))
            .Select(group => CreateExerciseCandidate(group.Key, group.Select(entry => entry.Key), exercisesByKey))
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
        return new ExerciseCandidate(
            CreatePrefixPattern(matchText, WordBoundary),
            ambiguous ? null : matches.SingleOrDefault(),
            ambiguous);
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

    public bool IsIgnored(string text) =>
        ignoredLabels.Any(pattern => pattern.IsMatch(text.TrimStart()));

    public IReadOnlyList<(WorkoutImportExercisePart Part, TextSpan? ConsumedSpan)> InterpretExerciseCell(string text)
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
                var result = InterpretExercisePart(part, out var consumedLength);
                TextSpan? span = result.ExerciseId is null
                    ? null
                    : new TextSpan(offset + part.Length - part.TrimStart().Length, consumedLength);
                results.Add((result, span));
            }

            offset += part.Length;
            if (offset < text.Length)
            {
                offset += separators.First(separator =>
                    text.AsSpan(offset).StartsWith(separator.AsSpan(), StringComparison.Ordinal)).Length;
            }
        }

        return results;
    }

    private WorkoutImportExercisePart InterpretExercisePart(string sourceText, out int consumedLength)
    {
        consumedLength = 0;
        var text = sourceText.TrimStart();
        var qualifierAttributes = new List<WorkoutImportAttributeAssignment>();
        var (candidate, exerciseOffset, matchEnd) = MatchExerciseWithLeadingQualifiers(text, qualifierAttributes);

        // if candidate is null we need to check if a combination of exercises

        if (candidate is null)
        {
            return new WorkoutImportExercisePart
            {
                SourceText = sourceText,
                Attributes = qualifierAttributes,
                Warning = $"No catalog exercise matches '{text.TrimEnd()}'."
            };
        }

        var matchedText = text[..matchEnd];
        var unconsumedText = ConsumeTrailingQualifiers(text[matchEnd..], qualifierAttributes);
        consumedLength = text.Length - unconsumedText.Length;
        var remainingText = unconsumedText.Trim();
        var attributes = CollectMatchedAttributes(qualifierAttributes, matchedText, text[exerciseOffset..matchEnd]);
        var exercise = candidate.Exercise;

        return new WorkoutImportExercisePart
        {
            SourceText = sourceText,
            MatchedText = matchedText,
            RemainingText = remainingText.Length == 0 ? null : remainingText,
            ExerciseName = exercise?.Name,
            ExerciseId = exercise?.Id,
            Attributes = attributes,
            Warning = GetExerciseMatchWarning(candidate, matchedText)
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
            var remainder = prefix[qualifierMatch.Length..].TrimStart();
            exerciseOffset = qualifierLimit - remainder.Length;

            if (bestCandidate is null)
            {
                var (candidate, length) = FindExercise(remainder);
                if (candidate is not null)
                {
                    return (candidate, exerciseOffset, exerciseOffset + length);
                }
            }
        }

        return (bestCandidate, exerciseOffset, matchLength);
    }

    private string ConsumeTrailingQualifiers(
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
        List<WorkoutImportAttributeAssignment> qualifierAttributes, string matchedText, string exerciseText) =>
        qualifierAttributes
            .Concat(aliasAttributes[FormatingUtilities.NormalizeNonNullString(matchedText)])
            .Concat(aliasAttributes[FormatingUtilities.NormalizeNullableString(exerciseText)])
            .DistinctBy(attribute => (attribute.Type, attribute.Value))
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

    private (ExerciseCandidate? Candidate, int Length) FindExercise(string text)
    {
        var match = candidates
            .Select(candidate => (Candidate: candidate, Length: candidate.Pattern.Match(text).Length))
            .DefaultIfEmpty()
            .MaxBy(match => match.Length);
        return match.Length > 0 ? match : default;
    }

    internal static bool TryReadDate(string? text, out DateTime date)
    {
        text = text?.Trim();
        var parsed = DateTime.TryParseExact(text, "d.M.yyyy", CultureInfo.InvariantCulture,
                         DateTimeStyles.None, out date)
                     || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out date)
                     && date.Date != DateTime.MinValue;
        date = date.Date;
        return parsed;
    }

    private static Regex CreatePrefixPattern(string text, string boundary)
    {
        var words = FormatingUtilities.NormalizeNonNullString(text).Split(' ');
        var pattern = string.Join(@"[^\S\r\n]+", words.Select(Regex.Escape));
        return new Regex($"^{pattern}{boundary}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private sealed record ExerciseCandidate(
        Regex Pattern,
        WorkoutImportExerciseReference? Exercise,
        bool Ambiguous);

    private sealed record Qualifier(Regex Pattern, IReadOnlyList<WorkoutImportAttributeAssignment> Attributes);
}
