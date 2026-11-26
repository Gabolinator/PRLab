using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class ExerciseInterpreter(
    WorkoutImportProfile profile, IReadOnlyList<WorkoutImportExerciseReference> exercises)
{
    private readonly IReadOnlyList<ExerciseCandidate> candidates = BuildExerciseCandidates(profile, exercises);

    public IReadOnlyList<ExerciseTextMatch> Interpret(string text)
    {
        var matches = new List<ExerciseTextMatch>();
        var (candidate, start, end) = FindNextExercise(text, 0);

        while (candidate is not null)
        {
            var span = new TextSpan(start, end - start);
            var matchedText = span.Read(text);
            var (nextCandidate, nextStart, nextEnd) = FindNextExercise(text, end);
            var remainingText = text[end..nextStart].Trim();
            var part = CreateExercisePart(text, candidate, matchedText, remainingText);
            matches.Add(new ExerciseTextMatch(part, span));
            (candidate, start, end) = (nextCandidate, nextStart, nextEnd);
        }

        return matches;
    }

    private static WorkoutImportExercisePart CreateExercisePart(
        string sourceText, ExerciseCandidate candidate, string matchedText, string remainingText) =>
        new()
        {
            SourceText = sourceText,
            MatchedText = matchedText,
            RemainingText = remainingText.Length == 0 ? null : remainingText,
            ExerciseName = candidate.Exercise?.Name,
            ExerciseId = candidate.Exercise?.Id,
            Warning = GetExerciseMatchWarning(candidate, matchedText)
        };

    private static string? GetExerciseMatchWarning(ExerciseCandidate candidate, string matchedText) =>
        candidate.Ambiguous
            ? $"Exercise name or alias '{matchedText}' is ambiguous."
            : candidate.Exercise is null
                ? $"Exercise alias '{matchedText}' points to a missing catalog exercise."
                : null;

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
        var pattern = WorkoutTextMatching.CreatePhrasePattern(matchText, WorkoutTextMatching.WordStart, WorkoutTextMatching.WordEnd);
        var exercise = ambiguous ? null : matches.SingleOrDefault();
        return new ExerciseCandidate(pattern, exercise, ambiguous);
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

}

internal sealed record ExerciseTextMatch(WorkoutImportExercisePart Part, TextSpan Span);
