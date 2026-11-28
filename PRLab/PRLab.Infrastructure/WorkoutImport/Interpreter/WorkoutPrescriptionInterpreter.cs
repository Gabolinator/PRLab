using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutPrescriptionInterpreter(IAppLogger logger, WorkoutImportProfile profile)
{
    private readonly LoadUnit preferredLoadUnit =
        profile.PreferredMeasurementSystem == MeasurementSystem.Imperial ? LoadUnit.Pound : LoadUnit.Kilogram;
    private static readonly Regex amrapPattern = new(
        $@"{WorkoutTextMatching.WordStart}amrap\s*{WorkoutPrescriptionSyntax.Duration}{WorkoutTextMatching.WordEnd}",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public IEnumerable<WorkoutWorkWindowSemantic> RecognizeWorkWindows(WorkoutSemanticSource source)
    {
        var matches = amrapPattern.Matches(source.Text);
        foreach (Match match in matches)
        {
            var offset = source.Offset + match.Index;
            if (!WorkoutPrescriptionSyntax.TryReadSeconds(match, out var durationSeconds))
            {
                LogWarning(source.Reference, match.Value, "Invalid AMRAP work window duration.");
                continue;
            }

            var matchedSource = source with { Text = match.Value, Offset = offset };
            var origin = new WorkoutSemanticOrigin(matchedSource, new WorkoutScopeSemanticTarget(WorkoutSemanticScope.Segment));
            yield return new WorkoutWorkWindowSemantic(origin, WorkMode.MaxWorkInTime, durationSeconds);
        }
    }

    public PrescriptionRecognition RecognizeToken(
        RawPrescriptionToken token,
        WorkoutSemanticSource source,
        ExercisePrescriptionContext? exerciseContext,
        IReadOnlySet<WorkoutInstructionKind> instructions,
        WorkoutWorkSemantic? cadenceWork)
    {
        var matchedSource = source with
        {
            Text = token.Span.Read(source.Text),
            Offset = source.Offset + token.Span.Start
        };
        WorkoutSemanticTarget target = exerciseContext is null
            ? new WorkoutScopeSemanticTarget(WorkoutSemanticScope.Row)
            : new WorkoutExerciseSemanticTarget(
                exerciseContext.Exercise.Id, exerciseContext.SourceReference, exerciseContext.SourceOffset);
        var origin = new WorkoutSemanticOrigin(matchedSource, target);
        var rowOrigin = origin with { Target = new WorkoutScopeSemanticTarget(WorkoutSemanticScope.Row) };

        var result = token switch
        {
            RawWorkToken work => RecognizeWork(work, origin, exerciseContext),
            RawRepLoadToken repLoad => RecognizeRepLoad(repLoad, origin, exerciseContext, preferredLoadUnit),
            RawCompactPairToken pair => RecognizeCompactPair(pair, origin, exerciseContext, instructions),
            RawPerSideHoldToken hold => RecognizePerSideHold(hold, source, origin, exerciseContext),
            RawZeroLoadPerSideToken unloaded => RecognizeZeroLoadPerSide(unloaded, origin, exerciseContext),
            RawClockToken clock when profile.Parsing.UseCompactSetCadence && cadenceWork is not null =>
                new PrescriptionRecognition(
                    [new WorkoutSetCadenceSemantic(origin with { Target = cadenceWork.Origin.Target }, clock.Seconds, cadenceWork.Origin)], null),
            RawClockToken => new PrescriptionRecognition([], null),
            RawCalorieRateToken rate when exerciseContext is not null =>
                new PrescriptionRecognition([new WorkoutCalorieRateSemantic(origin, rate.CaloriesPerHour)], null),
            RawCalorieRateToken => new PrescriptionRecognition([], null),
            RawLoadToken load => new PrescriptionRecognition([new WorkoutLoadSemantic(origin, load.Value,
                load.Unit.Equals("kg", StringComparison.OrdinalIgnoreCase) ? LoadUnit.Kilogram : LoadUnit.Pound)], null),
            RawDurationToken { Role: DurationRole.TimeCap } cap => new PrescriptionRecognition(
                [new WorkoutTimeCapSemantic(rowOrigin, cap.Seconds)], null),
            RawDurationToken rest => new PrescriptionRecognition(
                [new WorkoutRestSemantic(rowOrigin, rest.Seconds, rest.Role == DurationRole.MinimumRest)], null),
            RawInstructionToken => new PrescriptionRecognition(
                [new WorkoutInstructionSemantic(rowOrigin, WorkoutInstructionKind.SplitAnyhow)], null),
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };

        return result;
    }

    private static string QuoteText(string text) =>
        $"\"{text.ReplaceLineEndings("\\n")}\"";

    private static PrescriptionRecognition RecognizePerSideHold(
        RawPerSideHoldToken token,
        WorkoutSemanticSource source,
        WorkoutSemanticOrigin origin,
        ExercisePrescriptionContext? exerciseContext)
    {
        if (exerciseContext?.Exercise.AllowedWorkTargetTypes?.Contains(WorkTargetType.Repetitions) != true)
        {
            return new PrescriptionRecognition([], "Per-side repetitions with a hold require an exercise that allows repetitions.");
        }

        var repOrigin = origin with { Source = ReadSourceSpan(source, token.RepSpan) };
        var holdOrigin = origin with { Source = ReadSourceSpan(source, token.HoldSpan) };

        return new PrescriptionRecognition(
            [
                new WorkoutWorkSemantic(repOrigin, WorkTargetType.Repetitions, token.Reps, null, WorkTargetScope.PerSide),
                new WorkoutHoldSemantic(holdOrigin, token.HoldSeconds, repOrigin)
            ],
            null);
    }

    private PrescriptionRecognition RecognizeZeroLoadPerSide(
        RawZeroLoadPerSideToken token,
        WorkoutSemanticOrigin origin,
        ExercisePrescriptionContext? exerciseContext)
    {
        if (exerciseContext?.Exercise.AllowedWorkTargetTypes?.Contains(WorkTargetType.Repetitions) != true)
        {
            return new PrescriptionRecognition([], "The coach's zero-load notation requires an exercise that allows repetitions.");
        }

        return new PrescriptionRecognition(
            [
                new WorkoutWorkSemantic(origin, WorkTargetType.Repetitions, token.Reps, null, WorkTargetScope.PerSide),
                new WorkoutLoadSemantic(origin, 0, preferredLoadUnit)
            ],
            null);
    }

    private static WorkoutSemanticSource ReadSourceSpan(WorkoutSemanticSource source, TextSpan span) =>
        source with { Text = span.Read(source.Text), Offset = source.Offset + span.Start };

    private static PrescriptionRecognition RecognizeWork(
        RawWorkToken token,
        WorkoutSemanticOrigin origin,
        ExercisePrescriptionContext? exerciseContext)
    {
        if (token.RequiresFollowingExercise && exerciseContext is null)
        {
            return new PrescriptionRecognition([], "The quantity is missing an adjacent resolved exercise.");
        }

        var (target, value, warning) = WorkoutWorkTargetResolver.Resolve(token.Work, exerciseContext);
        if (target is null)
        {
            return new PrescriptionRecognition([], warning);
        }

        var work = new WorkoutWorkSemantic(origin, target.Value, value, token.Work.SetCount, token.Work.Scope);
        return new PrescriptionRecognition([work], warning);
    }

    private PrescriptionRecognition RecognizeRepLoad(
        RawRepLoadToken token,
        WorkoutSemanticOrigin origin,
        ExercisePrescriptionContext? exerciseContext,
        LoadUnit loadUnit)
    {
        var explicitSeriesRule = profile.Parsing.UseRepLoadSeries == true && token.Loads.Count > 1 &&
            exerciseContext?.Exercise.AllowedWorkTargetTypes?.Contains(WorkTargetType.Repetitions) == true;
        if (exerciseContext?.SupportsRepLoad != true && !explicitSeriesRule)
        {
            return new PrescriptionRecognition([], "Compact rep/load notation requires an exercise that supports repetitions and external load.");
        }

        var items = new List<WorkoutImportSemanticItem>();
        for (var index = 0; index < token.Loads.Count; index++)
        {
            var setOrigin = origin with { SeriesIndex = index };
            items.Add(new WorkoutWorkSemantic(setOrigin, WorkTargetType.Repetitions, token.Reps, token.Sets, WorkTargetScope.Total));
            items.Add(new WorkoutLoadSemantic(setOrigin, token.Loads[index], loadUnit));
        }

        return new PrescriptionRecognition(items, null);
    }

    private PrescriptionRecognition RecognizeCompactPair(
        RawCompactPairToken pair,
        WorkoutSemanticOrigin origin,
        ExercisePrescriptionContext? exerciseContext,
        IReadOnlySet<WorkoutInstructionKind> instructions)
    {
        var requiredInstruction = profile.Parsing.CompactPairRepLoadInstruction;
        if (requiredInstruction.HasValue && instructions.Contains(requiredInstruction.Value))
        {
            if (exerciseContext?.SupportsRepLoad != true)
            {
                return new PrescriptionRecognition([], "Compact rep/load notation requires an exercise that supports repetitions and external load.");
            }

            return new PrescriptionRecognition(
                [
                    new WorkoutWorkSemantic(origin, WorkTargetType.Repetitions, pair.First, null, WorkTargetScope.Total),
                    new WorkoutLoadSemantic(origin, pair.Second, preferredLoadUnit)
                ],
                null);
        }

        if (pair.IntegerSecond is not { } repetitions)
        {
            return new PrescriptionRecognition([], "Compact rep/load notation has no matching profile instruction.");
        }

        var work = new RawWorkoutWork(repetitions, string.Empty, pair.First, WorkTargetScope.Total);
        return RecognizeWork(new RawWorkToken(pair.Span, work, RequiresFollowingExercise: false), origin, exerciseContext);
    }

    private void LogWarning(string? sourceReference, string text, string warning)
    {
        if (logger.Enabled)
        {
            logger.LogWarning(
                nameof(WorkoutPrescriptionInterpreter),
                $"{sourceReference} · {QuoteText(text)} · {warning}");
        }
    }
}

internal sealed record PrescriptionRecognition(IReadOnlyList<WorkoutImportSemanticItem> Items, string? Warning);
