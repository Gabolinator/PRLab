using System.Globalization;
using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutPrescriptionInterpreter(IAppLogger logger)
{
    private const string Duration = @"(?<duration>[1-9][0-9]*)\s*(?<unit>s|sec(?:ond)?s?|m|min(?:ute)?s?|['′])";
    private const string Scope = @"(?<side>\s+each\s+side)?";
    private const string AdjacentClock = @"\s+[0-9]+:[0-5][0-9](?![\p{L}\p{N}:.])";

    private static readonly Regex workPattern = CreatePattern(
        @"(?:(?<sets>[1-9][0-9]*)\s*[x×]\s*)?(?<work>[1-9][0-9]*)" +
        @"(?:\s*(?<unit>reps?|s|sec(?:ond)?s?|min(?:ute)?s?|m|km|cal|['′]))?" + Scope,
        AdjacentClock);
    private static readonly Regex restPattern = CreatePattern(@"(?:rest\s+" + Duration + "|" + Duration + @"\s+rest)");
    private static readonly Regex timeCapPattern = CreatePattern(@"(?:time\s+cap|cap)\s+" + Duration);
    private static readonly Regex loadPattern = CreatePattern(@"(?:@\s*)?(?<load>[0-9]+(?:\.[0-9]+)?)\s*(?<unit>kg|lb|lbs)");
    private static readonly Regex splitPattern = CreatePattern(@"split\s+anyhow");

    public IEnumerable<(WorkoutImportPrescriptionDraft Draft, TextSpan Span)> Interpret(
        string text, string? sourceReference, int sourceOffset)
    {
        if (logger.Enabled)
        {
            logger.Log(nameof(WorkoutPrescriptionInterpreter),
                $"{sourceReference} · fragment at offset {sourceOffset} · {QuoteText(text)}");
        }

        var offset = 0;
        while (offset < text.Length)
        {
            var remainingText = text[offset..];
            if (InterpretPrefix(remainingText, sourceReference) is not { } result)
            {
                if (logger.Enabled)
                {
                    var remainingOffset = sourceOffset + offset;
                    logger.Log(nameof(WorkoutPrescriptionInterpreter),
                        $"{sourceReference} · stopped at offset {remainingOffset} · no supported prescription at the start of {QuoteText(remainingText)}");
                }

                yield break;
            }

            var span = result.Span.OffsetBy(offset);
            if (logger.Enabled)
            {
                var sourceSpan = span.OffsetBy(sourceOffset);
                LogMatch(result.Draft, sourceSpan);
            }

            yield return (result.Draft, span);
            offset = span.End;

            var suffix = text[offset..];
            var trimmedSuffix = suffix.TrimStart();
            offset += suffix.Length - trimmedSuffix.Length;
            if (trimmedSuffix.StartsWith(','))
            {
                offset++;
            }
        }
    }

    private static string QuoteText(string text) =>
        $"\"{text.ReplaceLineEndings("\\n")}\"";

    private (WorkoutImportPrescriptionDraft Draft, TextSpan Span)? InterpretPrefix(string text, string? sourceReference)
    {
        var rest = restPattern.Match(text);
        if (TryReadSeconds(rest, out var restSeconds))
        {
            var step = new WorkoutStepImportDraft
            {
                SourceText = rest.Value.Trim(),
                StepKind = WorkoutStepKind.Rest,
                RestSeconds = restSeconds,
                Confidence = 1
            };
            return CreateResult(rest, sourceReference, step);
        }
        LogRejectedMatch(rest, sourceReference, "rest duration");

        var timeCap = timeCapPattern.Match(text);
        if (TryReadSeconds(timeCap, out var capSeconds))
        {
            var step = new WorkoutStepImportDraft
            {
                SourceText = timeCap.Value.Trim(),
                StepKind = WorkoutStepKind.Instruction,
                Confidence = 1
            };
            return CreateResult(timeCap, sourceReference, step, capSeconds);
        }
        LogRejectedMatch(timeCap, sourceReference, "time cap");

        var work = workPattern.Match(text);
        if (TryReadWork(work) is { } workStep)
        {
            return CreateResult(work, sourceReference, workStep);
        }
        LogRejectedMatch(work, sourceReference, "work target");

        var load = loadPattern.Match(text);
        var loadText = load.Groups["load"].Value;
        if (decimal.TryParse(loadText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var loadValue))
        {
            var unit = load.Groups["unit"].Value;
            var step = new WorkoutStepImportDraft
            {
                SourceText = load.Value.Trim(),
                StepKind = WorkoutStepKind.Exercise,
                LoadValue = loadValue,
                LoadUnit = unit.Equals("kg", StringComparison.OrdinalIgnoreCase) ? LoadUnit.Kilogram : LoadUnit.Pound,
                Confidence = 1
            };
            return CreateResult(load, sourceReference, step);
        }
        LogRejectedMatch(load, sourceReference, "load");

        var split = splitPattern.Match(text);
        if (!split.Success)
        {
            return null;
        }

        var instruction = split.Value.Trim();
        var splitStep = new WorkoutStepImportDraft
        {
            SourceText = instruction,
            StepKind = WorkoutStepKind.Instruction,
            Notes = instruction,
            Confidence = 1
        };
        return CreateResult(split, sourceReference, splitStep);
    }

    private void LogMatch(WorkoutImportPrescriptionDraft draft, TextSpan span)
    {
        var step = draft.Step;
        logger.Log(nameof(WorkoutPrescriptionInterpreter),
            $"{draft.SourceReference} · matched {QuoteText(step.SourceText)} at offset {span.Start}, length {span.Length} · " +
            $"kind={step.StepKind}, sets={step.SetCount}, work={step.WorkValue}, target={step.WorkTargetType}, scope={step.Scope}, " +
            $"load={step.LoadValue}, load unit={step.LoadUnit}, rest seconds={step.RestSeconds}, time cap seconds={draft.TimeCapSeconds}, notes={step.Notes}");
    }

    private void LogRejectedMatch(Match match, string? sourceReference, string rule)
    {
        if (logger.Enabled && match.Success)
        {
            logger.Log(nameof(WorkoutPrescriptionInterpreter),
                $"{sourceReference} · {rule} syntax matched {QuoteText(match.Value)} but did not produce a valid value");
        }
    }

    private static Regex CreatePattern(string expression, string? adjacentToken = null)
    {
        const string delimiter = @"\s*(?:$|[,(|@])";
        var boundary = adjacentToken is null ? delimiter : $"{delimiter}|{adjacentToken}";
        return new Regex($@"^\s*(?:{expression})(?={boundary})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static (WorkoutImportPrescriptionDraft Draft, TextSpan Span) CreateResult(
        Match match, string? sourceReference, WorkoutStepImportDraft step, int? timeCapSeconds = null)
    {
        var start = match.Index + match.Length - match.Value.TrimStart().Length;
        var span = new TextSpan(start, match.Index + match.Length - start);
        return (new WorkoutImportPrescriptionDraft(sourceReference, step, timeCapSeconds), span);
    }

    private static bool TryReadSeconds(Match match, out int seconds)
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

    private static WorkoutStepImportDraft? TryReadWork(Match match)
    {
        var setsText = match.Groups["sets"].Value;
        var workText = match.Groups["work"].Value;
        var unit = match.Groups["unit"].Value.ToLowerInvariant();
        var perSide = match.Groups["side"].Success;
        if ((setsText.Length == 0 && unit.Length == 0 && !perSide) || !int.TryParse(workText, out var value))
        {
            return null;
        }

        int? setCount = null;
        if (setsText.Length > 0)
        {
            if (!int.TryParse(setsText, out var sets))
            {
                return null;
            }

            setCount = sets;
        }

        var target = unit switch
        {
            "m" or "km" => WorkTargetType.DistanceMeters,
            "cal" => WorkTargetType.Calories,
            "" or "rep" or "reps" => WorkTargetType.Repetitions,
            _ => WorkTargetType.DurationSeconds
        };
        var multiplier = unit switch
        {
            "km" => 1000,
            "'" or "′" => 60,
            _ when unit.StartsWith("min", StringComparison.Ordinal) => 60,
            _ => 1
        };
        return new WorkoutStepImportDraft
        {
            SourceText = match.Value.Trim(),
            StepKind = WorkoutStepKind.Exercise,
            WorkTargetType = target,
            WorkValue = (decimal)value * multiplier,
            Scope = perSide ? WorkTargetScope.PerSide : WorkTargetScope.Total,
            SetCount = setCount,
            Confidence = 1
        };
    }
}
