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

    public WorkoutInterpreterHelper(
        WorkoutImportProfile profile,
        IReadOnlyList<WorkoutImportExerciseReference> exercises)
    {
        separators = profile.Separators.Where(separator => !string.IsNullOrEmpty(separator)).ToArray();
        ignoredLabels = profile.IgnoredLabels
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => CreatePrefixPattern(label, @"(?=[^\S\r\n]*(?:$|\r|\n))"))
            .ToArray();

        var exercisesByKey = exercises.ToLookup(exercise => exercise.NameKey);
        var names = exercises.Select(exercise => (Text: exercise.Name, Key: exercise.NameKey));
        var aliases = profile.ExerciseAliases
            .SelectMany(entry => entry.Value.Select(alias =>
                (Text: alias, Key: FormatingUtilities.NormalizeNameKey(entry.Key))));

        candidates = names.Concat(aliases)
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Text))
            .GroupBy(entry => FormatingUtilities.NormalizeNonNullString(entry.Text))
            .Select(group =>
            {
                var keys = group.Select(entry => entry.Key).Distinct().ToList();
                var matches = keys.SelectMany(key => exercisesByKey[key]).ToList();
                var ambiguous = keys.Count > 1 || matches.Count > 1;
                return new ExerciseCandidate(
                    CreatePrefixPattern(group.Key, @"(?![\p{L}\p{M}\p{N}_])"),
                    ambiguous ? null : matches.SingleOrDefault(),
                    ambiguous);
            })
            .ToList();
    }

    public bool IsIgnored(string text) =>
        ignoredLabels.Any(pattern => pattern.IsMatch(text.TrimStart()));

    public IReadOnlyList<WorkoutImportExercisePart> InterpretExerciseCell(string text)
    {
        string[] parts = separators.Length == 0
            ? [text]
            : text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        return parts
            .Where(part => !string.IsNullOrWhiteSpace(part) && !IsIgnored(part))
            .Select(InterpretPart)
            .ToList();
    }

    private WorkoutImportExercisePart InterpretPart(string sourceText)
    {
        var text = sourceText.TrimStart();
        ExerciseCandidate? bestCandidate = null;
        var matchLength = 0;

        foreach (var candidate in candidates)
        {
            var match = candidate.Pattern.Match(text);
            if (match.Length > matchLength)
            {
                bestCandidate = candidate;
                matchLength = match.Length;
            }
        }

        if (bestCandidate is null)
        {
            return new WorkoutImportExercisePart
            {
                SourceText = sourceText,
                Warning = $"No catalog exercise matches '{text.TrimEnd()}'."
            };
        }

        var matchedText = text[..matchLength];
        var remainingText = text[matchLength..].Trim();
        var exercise = bestCandidate.Exercise;
        var warning = bestCandidate.Ambiguous
            ? $"Exercise name or alias '{matchedText}' is ambiguous."
            : exercise is null
                ? $"Exercise alias '{matchedText}' points to a missing catalog exercise."
                : null;

        return new WorkoutImportExercisePart
        {
            SourceText = sourceText,
            MatchedText = matchedText,
            RemainingText = remainingText.Length == 0 ? null : remainingText,
            ExerciseName = exercise?.Name,
            ExerciseId = exercise?.Id,
            Warning = warning
        };
    }

    internal static bool TryReadDate(string? text, out DateTime date)
    {
        text = text?.Trim();
        var parsed = DateTime.TryParseExact(text, "d.M.yyyy", CultureInfo.InvariantCulture,
                         DateTimeStyles.None, out date)
                     || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
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
}
