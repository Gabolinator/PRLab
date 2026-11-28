using System.Globalization;
using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutSetupRecognizer(WorkoutImportProfile profile)
{
    private static readonly Regex boxHeightPattern = new(
        @"(?<![\p{L}\p{N}_.-])(?<height>[0-9]+(?:\.[0-9]+)?)\s*cm(?![\p{L}\p{N}_/])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public IEnumerable<WorkoutImportSemanticItem> FindExerciseSetup(
        WorkoutSemanticSource source,
        WorkoutExerciseSemanticTarget target,
        string? boxName)
    {
        foreach (var (match, name) in FindAliases(source.Text, profile.Aliases.EquipmentOn))
        {
            yield return new WorkoutEquipmentSemantic(CreateOrigin(source, target, match), name, WorkoutEquipmentRelation.On);
        }

        var countedPrefix = WorkoutTextMatching.WordStart + @"(?<quantity>[1-9][0-9]*)\s*";
        foreach (var (match, name) in FindAliases(source.Text, profile.Aliases.CountedEquipment, countedPrefix))
        {
            if (int.TryParse(match.Groups["quantity"].Value, out var quantity))
            {
                yield return new WorkoutEquipmentSemantic(CreateOrigin(source, target, match), name, Quantity: quantity);
            }
        }

        foreach (var (match, quality) in FindAliases(source.Text, profile.Aliases.MovementQualityCues))
        {
            yield return new WorkoutMovementQualitySemantic(CreateOrigin(source, target, match), quality);
        }

        foreach (var match in FindPhrases(source.Text, profile.Aliases.UnpositionedPauses))
        {
            yield return new WorkoutPauseSemantic(CreateOrigin(source, target, match), null);
        }

        foreach (var match in FindPhrases(source.Text, profile.Aliases.SelfPaced))
        {
            yield return new WorkoutSelfPacedSemantic(CreateOrigin(source, target, match));
        }

        if (boxName is not null)
        {
            foreach (Match match in boxHeightPattern.Matches(source.Text))
            {
                var heightText = match.Groups["height"].Value;
                if (decimal.TryParse(heightText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var height) &&
                    height > 0)
                {
                    yield return new WorkoutEquipmentSemantic(CreateOrigin(source, target, match), boxName, HeightCentimeters: height);
                }
            }
        }
    }

    public IReadOnlyList<WorkoutImportSemanticItem> RecognizeBlockHeading(WorkoutSemanticSource source)
    {
        var target = new WorkoutScopeSemanticTarget(WorkoutSemanticScope.Block);
        var blockTypes =
            FindAliases(source.Text, profile.Mappings.BlockTypes)
                .Select(entry => new WorkoutBlockTypeSemantic(CreateOrigin(source, target, entry.Match), entry.Value))
                .ToList();
        if (blockTypes.Count != 1)
        {
            return [];
        }

        var intensities = FindAliases(source.Text, profile.Mappings.Intensities)
            .Select(entry => new WorkoutIntensitySemantic(CreateOrigin(source, target, entry.Match), entry.Value));
        var headingItems =
            blockTypes.Cast<WorkoutImportSemanticItem>()
                .Concat(intensities)
                .OrderBy(item => item.Origin.Source.Offset)
                .ToList();
        var offset = 0;
        foreach (var item in headingItems)
        {
            var start = item.Origin.Source.Offset - source.Offset;
            if (start < offset || !string.IsNullOrWhiteSpace(source.Text[offset..start]))
            {
                return [];
            }

            offset = start + item.Origin.Source.Length;
        }

        return string.IsNullOrWhiteSpace(source.Text[offset..]) ? headingItems : [];
    }

    private static IEnumerable<(Match Match, TValue Value)> FindAliases<TValue>(
        string text,
        IReadOnlyDictionary<string, TValue> aliases,
        string start = WorkoutTextMatching.WordStart)
    {
        foreach (var (phrase, value) in aliases)
        {
            if (string.IsNullOrWhiteSpace(phrase))
            {
                continue;
            }

            var pattern = WorkoutTextMatching.CreatePhrasePattern(phrase, start, WorkoutTextMatching.WordEnd);
            foreach (Match match in pattern.Matches(text))
            {
                yield return (match, value);
            }
        }
    }

    private static IEnumerable<Match> FindPhrases(string text, IReadOnlyList<string> phrases)
    {
        foreach (var phrase in phrases)
        {
            if (string.IsNullOrWhiteSpace(phrase))
            {
                continue;
            }

            var pattern = WorkoutTextMatching.CreatePhrasePattern(phrase, WorkoutTextMatching.WordStart, WorkoutTextMatching.WordEnd);
            foreach (Match match in pattern.Matches(text))
            {
                yield return match;
            }
        }
    }

    private static WorkoutSemanticOrigin CreateOrigin(WorkoutSemanticSource source, WorkoutSemanticTarget target, Match match)
    {
        var matchSource = source with { Text = match.Value, Offset = source.Offset + match.Index };

        return new WorkoutSemanticOrigin(matchSource, target);
    }
}
