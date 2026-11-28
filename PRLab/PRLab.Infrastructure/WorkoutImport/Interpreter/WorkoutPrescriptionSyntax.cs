using System.Globalization;
using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal static class WorkoutPrescriptionSyntax
{
    public const string Duration = @"(?<duration>[1-9][0-9]*)\s*(?<unit>s|sec(?:ond)?s?|m|min(?:ute)?s?|['′])";

    private static readonly Regex restPattern = CreatePattern(
        @"(?:rest\s+" + Duration + @"(?<minimum>\+)?" + "|" + Duration + @"(?<minimum>\+)?\s+rest)");
    private static readonly Regex capPattern = CreatePattern(@"(?:time\s+cap|cap)\s+" + Duration);
    private static readonly Regex loadPattern = CreatePattern(@"(?:@\s*)?(?<load>[0-9]+(?:\.[0-9]+)?)\s*(?<unit>kg|lbs?)");
    private static readonly Regex splitPattern = CreatePattern(@"split\s+anyhow");
    private static readonly Regex attachedQuantityPattern = CreatePattern(@"(?<work>[1-9][0-9]*)\s*\*");
    private static readonly Regex workPattern = CreatePattern(
        @"(?:(?<sets>[1-9][0-9]*)\s*[x×]\s*)?(?<work>[1-9][0-9]*)" +
        @"(?:\s*(?<unit>reps?|s|sec(?:ond)?s?|min(?:ute)?s?|m|km|cal|['′]))?(?<side>\s+each\s+side)?");
    private static readonly Regex bareQuantitySuffix = new(
        @"\G\s*(?:$|split\s+anyhow\b)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex unsupportedComposite = new(
        @"\G[0-9]+\s*[x×]\s*[0-9]+(?:\.[0-9]+|(?:\s*[x×-]\s*[0-9]+)+)(?:\.[0-9]+)?",
        RegexOptions.CultureInvariant);
    private const string Count = @"[1-9][0-9]*";
    private const string Weight = @"[0-9]+(?:\.[0-9]+)?";
    private const string Times = @"\s*[x×]\s*";
    private static readonly Regex compactLoadPattern = CreatePattern(
        $@"(?:(?<sets>{Count}){Times}(?<reps>{Count}){Times}(?<load>{Weight})|" +
        $@"(?<reps>{Count}){Times}(?<load>{Weight})(?:\s*-\s*(?<load>{Weight}))+)(?!\s*[x×.\-]\s*[0-9])");
    private static readonly Regex compactPairPattern = CreatePattern(
        $@"(?<first>{Count}){Times}(?<second>{Weight})(?!\s*[x×.\-]\s*[0-9])");
    private static readonly Regex clockPattern = CreatePattern(@"(?<minutes>[0-9]+):(?<seconds>[0-5][0-9])");
    private static readonly Regex calorieRatePattern = CreatePattern($@"(?<rate>{Weight})\s*cal\s*/\s*h");
    private static readonly Regex perSideHoldPattern = CreatePattern(
        $@"(?<repetitionPhrase>(?<reps>{Count})\s*[x×]\s*each\s+side)\s+(?<holdPhrase>{Duration}\s+hold)");
    private static readonly Regex zeroLoadPerSidePattern = CreatePattern(
        $@"(?<reps>{Count}){Times}0\s+each\s+side");

    public static IEnumerable<RawPrescriptionToken> Scan(string text, WorkoutImportParsing parsing)
    {
        var firstContent = text.Length - text.TrimStart().Length;
        var offset = firstContent;
        while (offset < text.Length)
        {
            if (!IsTokenStart(text, offset))
            {
                offset++;
                continue;
            }

            var hold = perSideHoldPattern.Match(text, offset);
            if (int.TryParse(hold.Groups["reps"].Value, out var holdReps) && TryReadSeconds(hold, out var holdSeconds))
            {
                var holdToken = new RawPerSideHoldToken(
                    GetSpan(hold),
                    holdReps,
                    holdSeconds,
                    GetSpan(hold.Groups["repetitionPhrase"]),
                    GetSpan(hold.Groups["holdPhrase"]));
                yield return holdToken;
                offset = holdToken.Span.End;
                continue;
            }

            var zeroLoad = zeroLoadPerSidePattern.Match(text, offset);
            if (zeroLoad.Success)
            {
                if (parsing.UseZeroLoadPerSide && int.TryParse(zeroLoad.Groups["reps"].Value, out var zeroLoadReps))
                {
                    yield return new RawZeroLoadPerSideToken(GetSpan(zeroLoad), zeroLoadReps);
                }

                offset += zeroLoad.Length;
                continue;
            }

            if (ReadCompactLoad(text, offset) is { } compactLoad &&
                (compactLoad.Loads.Count > 1
                    ? parsing.UseRepLoadSeries ?? parsing.UseCompactRepLoadSyntax
                    : parsing.UseCompactRepLoadSyntax))
            {
                yield return compactLoad;
                offset = compactLoad.Span.End;
                continue;
            }

            if (parsing.UseCompactRepLoadSyntax && ReadCompactPair(text, offset) is { } compactPair)
            {
                yield return compactPair;
                offset = compactPair.Span.End;
                continue;
            }

            var composite = unsupportedComposite.Match(text, offset);
            if (composite.Success)
            {
                offset += composite.Length;
                continue;
            }

            var token = ReadToken(text, offset, firstContent);
            if (token is null)
            {
                offset++;
                continue;
            }

            yield return token;
            offset = token.Span.End;
        }
    }

    private static RawCompactPairToken? ReadCompactPair(string text, int offset)
    {
        var match = compactPairPattern.Match(text, offset);
        var explicitWork = workPattern.Match(text, offset);
        var secondText = match.Groups["second"].Value;
        if (explicitWork.Length > match.Length ||
            !int.TryParse(match.Groups["first"].Value, out var first) ||
            !decimal.TryParse(secondText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var second))
        {
            return null;
        }

        int? integerSecond = int.TryParse(secondText, out var repetitions) && repetitions > 0 ? repetitions : null;
        return new RawCompactPairToken(GetSpan(match), first, second, integerSecond);
    }

    private static RawRepLoadToken? ReadCompactLoad(string text, int offset)
    {
        var match = compactLoadPattern.Match(text, offset);
        if (!int.TryParse(match.Groups["reps"].Value, out var reps))
        {
            return null;
        }

        var sets = 1;
        var setsGroup = match.Groups["sets"];
        if (setsGroup.Success && !int.TryParse(setsGroup.Value, out sets))
        {
            return null;
        }

        var loads = new List<decimal>();
        foreach (Capture load in match.Groups["load"].Captures)
        {
            if (!decimal.TryParse(load.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
            {
                return null;
            }

            loads.Add(value);
        }

        return new RawRepLoadToken(GetSpan(match), sets, reps, loads);
    }

    public static bool TryReadSeconds(Match match, out int seconds)
    {
        seconds = default;
        var durationText = match.Groups["duration"].Value;
        var unit = match.Groups["unit"].Value;
        var multiplier = unit.StartsWith("s", StringComparison.OrdinalIgnoreCase) ? 1 : 60;
        if (!int.TryParse(durationText, out var duration) || duration > int.MaxValue / multiplier)
        {
            return false;
        }

        seconds = duration * multiplier;
        return true;
    }

    private static RawPrescriptionToken? ReadToken(string text, int offset, int firstContent)
    {
        var rate = calorieRatePattern.Match(text, offset);
        if (decimal.TryParse(rate.Groups["rate"].Value, NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var caloriesPerHour) && caloriesPerHour > 0)
        {
            return new RawCalorieRateToken(GetSpan(rate), caloriesPerHour);
        }

        var clock = clockPattern.Match(text, offset);
        if (int.TryParse(clock.Groups["minutes"].Value, out var minutes) &&
            int.TryParse(clock.Groups["seconds"].Value, out var seconds) &&
            minutes <= (int.MaxValue - seconds) / 60 && minutes * 60 + seconds > 0)
        {
            return new RawClockToken(GetSpan(clock), minutes * 60 + seconds);
        }

        var rest = restPattern.Match(text, offset);
        if (TryReadSeconds(rest, out var restSeconds))
        {
            return new RawDurationToken(GetSpan(rest), restSeconds,
                rest.Groups["minimum"].Success ? DurationRole.MinimumRest : DurationRole.Rest);
        }

        var cap = capPattern.Match(text, offset);
        if (TryReadSeconds(cap, out var capSeconds))
        {
            return new RawDurationToken(GetSpan(cap), capSeconds, DurationRole.TimeCap);
        }

        var load = loadPattern.Match(text, offset);
        var loadText = load.Groups["load"].Value;
        if (decimal.TryParse(loadText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var loadValue))
        {
            return new RawLoadToken(GetSpan(load), loadValue, load.Groups["unit"].Value);
        }

        var split = splitPattern.Match(text, offset);
        if (split.Success)
        {
            return new RawInstructionToken(GetSpan(split), WorkoutInstructionKind.SplitAnyhow);
        }

        var attached = attachedQuantityPattern.Match(text, offset);
        if (TryReadWork(attached) is { } attachedQuantity)
        {
            return new RawWorkToken(GetSpan(attached), attachedQuantity, RequiresFollowingExercise: true);
        }

        var work = workPattern.Match(text, offset);
        if (TryReadWork(work) is not { } quantity)
        {
            return null;
        }

        var isBare = quantity.Unit.Length == 0 && !quantity.SetCount.HasValue && quantity.Scope == WorkTargetScope.Total;
        var workEnd = offset + work.Length;
        if (isBare && (offset != firstContent || !bareQuantitySuffix.IsMatch(text, workEnd)))
        {
            return null;
        }

        return new RawWorkToken(GetSpan(work), quantity, RequiresFollowingExercise: false);
    }

    private static RawWorkoutWork? TryReadWork(Match match)
    {
        if (!int.TryParse(match.Groups["work"].Value, out var value))
        {
            return null;
        }

        int? setCount = null;
        var sets = match.Groups["sets"];
        if (sets.Success)
        {
            if (!int.TryParse(sets.Value, out var parsedSets))
            {
                return null;
            }

            setCount = parsedSets;
        }

        var unit = match.Groups["unit"].Value.ToLowerInvariant();
        var scope = match.Groups["side"].Success ? WorkTargetScope.PerSide : WorkTargetScope.Total;
        return new RawWorkoutWork(value, unit, setCount, scope);
    }

    private static Regex CreatePattern(string expression) =>
        new($@"\G(?:{expression})(?=$|[\s,();|@])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static bool IsTokenStart(string text, int offset) =>
        offset == 0 || !(char.IsLetterOrDigit(text[offset - 1]) || text[offset - 1] is '_' or '.' or ':' or '-' or '/' or '×' or '+');

    private static TextSpan GetSpan(Capture match) => new(match.Index, match.Length);
}

internal abstract record RawPrescriptionToken(TextSpan Span);

internal sealed record RawWorkToken(TextSpan Span, RawWorkoutWork Work, bool RequiresFollowingExercise)
    : RawPrescriptionToken(Span);

internal sealed record RawRepLoadToken(TextSpan Span, int Sets, int Reps, IReadOnlyList<decimal> Loads)
    : RawPrescriptionToken(Span);

internal sealed record RawCompactPairToken(TextSpan Span, int First, decimal Second, int? IntegerSecond)
    : RawPrescriptionToken(Span);

internal sealed record RawClockToken(TextSpan Span, int Seconds) : RawPrescriptionToken(Span);

internal sealed record RawPerSideHoldToken(
    TextSpan Span, int Reps, int HoldSeconds, TextSpan RepSpan, TextSpan HoldSpan) : RawPrescriptionToken(Span);

internal sealed record RawZeroLoadPerSideToken(TextSpan Span, int Reps) : RawPrescriptionToken(Span);

internal sealed record RawCalorieRateToken(TextSpan Span, decimal CaloriesPerHour) : RawPrescriptionToken(Span);

internal sealed record RawLoadToken(TextSpan Span, decimal Value, string Unit) : RawPrescriptionToken(Span);

internal sealed record RawDurationToken(TextSpan Span, int Seconds, DurationRole Role) : RawPrescriptionToken(Span);

internal sealed record RawInstructionToken(TextSpan Span, WorkoutInstructionKind Instruction) : RawPrescriptionToken(Span);

internal enum DurationRole
{
    Rest,
    MinimumRest,
    TimeCap
}
