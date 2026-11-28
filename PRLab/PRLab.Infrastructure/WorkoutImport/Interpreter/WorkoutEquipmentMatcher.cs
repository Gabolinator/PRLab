using System.Globalization;
using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Utilities;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutEquipmentMatcher
{
    private readonly ILookup<string, WorkoutImportEquipmentReference> canonicalNames;
    private readonly ILookup<string, WorkoutImportEquipmentReference> domainAliases;
    private readonly ILookup<string, ProfileAlias> profileAliases;
    private readonly IReadOnlyList<(string Key, Regex Pattern)> patterns;

    public WorkoutEquipmentMatcher(WorkoutImportContext context)
    {
        canonicalNames = context.Equipment.ToLookup(equipment => equipment.NameKey);
        domainAliases =
            context.Equipment
                .SelectMany(equipment => equipment.Aliases.Select(alias => (Key: Normalize(alias), Equipment: equipment)))
                .ToLookup(entry => entry.Key, entry => entry.Equipment);
        profileAliases = BuildProfileAliases(context.Profile.Aliases);

        patterns =
            canonicalNames.Select(group => group.Key)
                .Concat(domainAliases.Select(group => group.Key))
                .Concat(profileAliases.Select(group => group.Key))
                .Where(key => key.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .Select(key => (key, CreatePattern(key)))
                .ToList();
    }

    public IEnumerable<EquipmentTextMatch> FindMatches(WorkoutSemanticSource source)
    {
        var matchesInTextOrder =
            patterns
                .SelectMany(entry => entry.Pattern.Matches(source.Text)
                    .Select(match => (entry.Key, Match: match)))
                .OrderBy(entry => entry.Match.Index)
                .ThenByDescending(entry => entry.Match.Length);

        var previousEnd = 0;
        foreach (var entry in matchesInTextOrder)
        {
            var match = entry.Match;
            if (match.Index < previousEnd)
            {
                continue;
            }

            var result = ResolveMatch(source, entry.Key, match);
            if (result is not null)
            {
                yield return result;
                previousEnd = match.Index + match.Length;
            }
        }
    }

    public WorkoutEquipmentSemantic ResolveConfiguredName(WorkoutEquipmentSemantic equipment)
    {
        var candidates = ResolveDomainIdentity(Normalize(equipment.Name));
        return candidates.Count == 1
            ? equipment with { EquipmentId = candidates[0].Id, Name = candidates[0].Name }
            : equipment;
    }

    private static string Normalize(string value) => FormatingUtilities.NormalizeNameKey(value);

    private static Regex CreatePattern(string key)
    {
        const string onPrefix = @"(?<on>on[^\S\r\n]+)?";
        const string quantityPrefix = @"(?:(?<![.\-])(?<quantity>[1-9][0-9]*)[^\S\r\n]*)?";
        const string separators = @"(?:[^\S\r\n]|[-_])*";
        var phrase = string.Join(separators, key.Select(character => Regex.Escape(character.ToString())));

        return new Regex(
            $@"{WorkoutTextMatching.WordStart}{onPrefix}{quantityPrefix}(?<label>{phrase}){WorkoutTextMatching.WordEnd}",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static ILookup<string, ProfileAlias> BuildProfileAliases(WorkoutImportAliases aliases)
    {
        var hints =
            aliases.PrescriptionHints
                .Where(hint => !string.IsNullOrWhiteSpace(hint.Key))
                .ToLookup(hint => Normalize(hint.Key), hint => hint.Value);
        var configuredAliases =
            aliases.Equipment.Select(alias => new ProfileAlias(alias.Key, alias.Value))
                .Concat(aliases.CountedEquipment.Select(alias => new ProfileAlias(alias.Key, alias.Value, RequiresQuantity: true)))
                .Concat(aliases.EquipmentOn.Select(alias => new ProfileAlias(alias.Key, alias.Value, IsOn: true)));
        var exerciseAliasesWithHints =
            aliases.Exercises
                .SelectMany(exercise => exercise.Value
                    .Where(phrase => !string.IsNullOrWhiteSpace(phrase) && hints.Contains(Normalize(phrase)))
                    .Select(phrase => new ProfileAlias(phrase, exercise.Key, RequiresDomainIdentity: true)));

        return configuredAliases
            .Concat(exerciseAliasesWithHints)
            .Where(alias => !string.IsNullOrWhiteSpace(alias.Phrase) && !string.IsNullOrWhiteSpace(alias.Name))
            .SelectMany(alias => hints[Normalize(alias.Phrase)]
                .Select(hint => (WorkTargetType?)hint)
                .DefaultIfEmpty()
                .Select(hint => alias with { Hint = hint }))
            .ToLookup(alias => Normalize(alias.Phrase));
    }

    private static EquipmentTextMatch CreateMatch(
        WorkoutSemanticSource source, Match match, IReadOnlyList<EquipmentCandidate> candidates, int? quantity)
    {
        var matchedSource = source with { Text = match.Value, Offset = source.Offset + match.Index };
        var label = match.Groups["label"];
        var labelSpan = new TextSpan(source.Offset + label.Index, label.Length);
        var identities = candidates.DistinctBy(candidate => (candidate.Equipment?.Id, Normalize(candidate.Name))).ToList();
        if (identities.Count != 1)
        {
            var names = string.Join(", ", identities.Select(candidate => candidate.Name));
            return new EquipmentTextMatch(matchedSource, labelSpan, null, $"Ambiguous equipment '{match.Value}': {names}.");
        }

        var identity = identities[0];
        var matchingProfiles = candidates.Where(candidate =>
            candidate.Equipment?.Id == identity.Equipment?.Id && Normalize(candidate.Name) == Normalize(identity.Name));
        var hints = matchingProfiles.Select(candidate => candidate.Hint).OfType<WorkTargetType>().Distinct().ToList();
        var relation = match.Groups["on"].Success || matchingProfiles.Any(candidate => candidate.IsOn)
            ? WorkoutEquipmentRelation.On
            : (WorkoutEquipmentRelation?)null;
        var origin = new WorkoutSemanticOrigin(matchedSource, new WorkoutScopeSemanticTarget(WorkoutSemanticScope.Equipment));
        var equipment = new WorkoutEquipmentSemantic(origin, identity.Name, relation, quantity)
        {
            EquipmentId = identity.Equipment?.Id,
            WorkTargetHint = hints.Count == 1 ? hints[0] : null
        };

        return new EquipmentTextMatch(matchedSource, labelSpan, equipment, null);
    }

    private IReadOnlyList<WorkoutImportEquipmentReference> ResolveDomainIdentity(string key)
    {
        var matches = canonicalNames.Contains(key) ? canonicalNames[key] : domainAliases[key];
        return matches.DistinctBy(equipment => equipment.Id).ToList();
    }

    private EquipmentTextMatch? ResolveMatch(WorkoutSemanticSource source, string key, Match match)
    {
        var quantity = int.TryParse(match.Groups["quantity"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var count)
            ? count
            : (int?)null;
        if (match.Groups["quantity"].Success && quantity is null)
        {
            return null;
        }

        var domainMatches = ResolveDomainIdentity(key);
        var profileMatches =
            profileAliases[key]
                .Where(alias => !alias.RequiresQuantity || quantity.HasValue)
                .SelectMany(ResolveProfileAlias);
        var candidates = domainMatches.Count == 0
            ? profileMatches.ToList()
            : domainMatches
                .Select(equipment => new EquipmentCandidate(equipment.Name, equipment))
                .Concat(profileMatches.Where(candidate => domainMatches.Any(equipment => equipment.Id == candidate.Equipment?.Id)))
                .ToList();

        return candidates.Count == 0 ? null : CreateMatch(source, match, candidates, quantity);
    }

    private IEnumerable<EquipmentCandidate> ResolveProfileAlias(ProfileAlias alias)
    {
        var matches = ResolveDomainIdentity(Normalize(alias.Name));
        if (matches.Count == 0 && !alias.RequiresDomainIdentity)
        {
            yield return new EquipmentCandidate(alias.Name, null, alias.Hint, alias.IsOn);
        }

        foreach (var equipment in matches)
        {
            yield return new EquipmentCandidate(equipment.Name, equipment, alias.Hint, alias.IsOn);
        }
    }

    private sealed record ProfileAlias(
        string Phrase, string Name, bool RequiresQuantity = false, bool IsOn = false,
        bool RequiresDomainIdentity = false, WorkTargetType? Hint = null);

    private sealed record EquipmentCandidate(
        string Name, WorkoutImportEquipmentReference? Equipment, WorkTargetType? Hint = null, bool IsOn = false);
}

internal sealed record EquipmentTextMatch(
    WorkoutSemanticSource Source, TextSpan LabelSpan, WorkoutEquipmentSemantic? Equipment, string? Warning);
