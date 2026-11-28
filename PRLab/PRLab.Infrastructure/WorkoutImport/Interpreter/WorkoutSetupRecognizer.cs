using System.Globalization;
using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Equipment;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutSetupRecognizer(WorkoutImportProfile profile)
{
    private static readonly Regex heightPattern = new(
        @"(?<![\p{L}\p{N}_.-])(?<height>[0-9]+(?:\.[0-9]+)?)\s*cm(?![\p{L}\p{N}_/])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static IEnumerable<EquipmentHeightTextMatch> FindEquipmentHeights(WorkoutSemanticSource source)
    {
        foreach (Match match in heightPattern.Matches(source.Text))
        {
            var heightText = match.Groups["height"].Value;
            if (decimal.TryParse(heightText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var height) &&
                height > 0)
            {
                var matchedSource = source with { Text = match.Value, Offset = source.Offset + match.Index };
                yield return new EquipmentHeightTextMatch(matchedSource, height);
            }
        }
    }

    public static (WorkoutEquipmentSemantic? Equipment, string? Warning) RecognizeEquipmentHeight(
        EquipmentHeightTextMatch match,
        WorkoutExerciseSemanticTarget? target,
        IReadOnlyList<WorkoutImportEquipmentReference> equipment)
    {
        var configurableEquipment =
            equipment
                .Where(reference => reference.SupportedConfigurationTypes.Contains(EquipmentConfigurationType.Height))
                .ToList();
        if (configurableEquipment.Count == 0)
        {
            return (null, null);
        }

        if (configurableEquipment.Count > 1)
        {
            var names = string.Join(", ", configurableEquipment.Select(reference => reference.Name));
            return (null, $"Ambiguous equipment height '{match.Source.Text}': {names}.");
        }

        var reference = configurableEquipment[0];
        WorkoutSemanticTarget semanticTarget = target is null
            ? new WorkoutScopeSemanticTarget(WorkoutSemanticScope.Equipment)
            : target;
        var origin = new WorkoutSemanticOrigin(match.Source, semanticTarget);
        var configuredEquipment = new WorkoutEquipmentSemantic(origin, reference.Name, HeightCentimeters: match.Centimeters)
        {
            EquipmentId = reference.Id
        };

        return (configuredEquipment, null);
    }

    public IEnumerable<WorkoutImportSemanticItem> FindExerciseCues(
        WorkoutSemanticSource source,
        WorkoutExerciseSemanticTarget target)
    {
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
        IReadOnlyDictionary<string, TValue> aliases)
    {
        foreach (var (phrase, value) in aliases)
        {
            if (string.IsNullOrWhiteSpace(phrase))
            {
                continue;
            }

            var pattern = WorkoutTextMatching.CreatePhrasePattern(
                phrase,
                WorkoutTextMatching.WordStart,
                WorkoutTextMatching.WordEnd);
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

internal sealed record EquipmentHeightTextMatch(WorkoutSemanticSource Source, decimal Centimeters);
