using System.Text.RegularExpressions;
using PRLab.Application.Models.ImportDraft;
using PRLab.Domain.Model.Value.Enum;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal sealed class WorkoutPrescriptionInterpreter(IAppLogger logger, MeasurementSystem measurementSystem)
{
    private readonly LoadUnit preferredLoadUnit =
        measurementSystem == MeasurementSystem.Imperial ? LoadUnit.Pound : LoadUnit.Kilogram;
    private static readonly Regex amrapPattern = new(
        $@"{WorkoutTextMatching.WordStart}amrap\s*{WorkoutPrescriptionSyntax.Duration}{WorkoutTextMatching.WordEnd}",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public IEnumerable<(WorkoutWorkWindowDraft Draft, TextSpan Span)> InterpretWorkWindows(
        string text, string? sourceReference, int sourceOffset)
    {
        var matches = amrapPattern.Matches(text);
        foreach (Match match in matches)
        {
            if (!WorkoutPrescriptionSyntax.TryReadSeconds(match, out var durationSeconds))
            {
                LogWarning(sourceReference, sourceOffset + match.Index, match.Value, "Invalid AMRAP work window duration.");
                continue;
            }

            var span = new TextSpan(match.Index, match.Length);
            var draft = new WorkoutWorkWindowDraft(WorkMode.MaxWorkInTime, durationSeconds, match.Value, sourceReference);
            if (logger.Enabled)
            {
                var absoluteOffset = sourceOffset + span.Start;
                logger.Log(nameof(WorkoutPrescriptionInterpreter),
                    $"{sourceReference} · matched workout work window {QuoteText(match.Value)} at offset {absoluteOffset} · " +
                    $"mode={draft.Mode}, duration seconds={draft.DurationSeconds}");
            }

            yield return (draft, span);
        }
    }

    public PrescriptionResult InterpretToken(
        RawPrescriptionToken token, string text, string? sourceReference, int sourceOffset,
        ExercisePrescriptionContext? exerciseContext)
    {
        var tokenText = token.Span.Read(text);
        var result = token switch
        {
            RawWorkToken work => ResolveWork(work, tokenText, sourceReference, exerciseContext),
            RawRepLoadToken repLoad => ResolveRepLoad(repLoad, tokenText, sourceReference, exerciseContext, preferredLoadUnit),
            RawLoadToken load => new PrescriptionResult([CreateDraft(sourceReference, new WorkoutStepImportDraft
            {
                SourceText = tokenText,
                StepKind = WorkoutStepKind.Exercise,
                ExerciseName = exerciseContext?.Exercise.Name,
                LoadValue = load.Value,
                LoadUnit = load.Unit.Equals("kg", StringComparison.OrdinalIgnoreCase) ? LoadUnit.Kilogram : LoadUnit.Pound,
                Confidence = 1
            }, exerciseContext)], null),
            RawDurationToken duration => new PrescriptionResult([CreateDurationDraft(duration, tokenText, sourceReference)], null),
            RawInstructionToken => new PrescriptionResult([CreateDraft(sourceReference, new WorkoutStepImportDraft
            {
                SourceText = tokenText,
                StepKind = WorkoutStepKind.Instruction,
                Notes = tokenText,
                Confidence = 1
            })], null),
            _ => throw new ArgumentOutOfRangeException(nameof(token))
        };

        if (logger.Enabled)
        {
            var sourceSpan = token.Span.OffsetBy(sourceOffset);
            foreach (var draft in result.Drafts)
            {
                LogMatch(draft, sourceSpan);
            }

            if (result.Warning is { } warning)
            {
                LogWarning(sourceReference, sourceSpan.Start, tokenText, warning);
            }
        }

        return result;
    }

    public void LogFragment(string text, string? sourceReference, int sourceOffset)
    {
        if (logger.Enabled)
        {
            logger.Log(nameof(WorkoutPrescriptionInterpreter),
                $"{sourceReference} · fragment at offset {sourceOffset} · {QuoteText(text)}");
        }
    }

    private static string QuoteText(string text) =>
        $"\"{text.ReplaceLineEndings("\\n")}\"";

    private static WorkoutImportPrescriptionDraft CreateDraft(
        string? sourceReference, WorkoutStepImportDraft step, ExercisePrescriptionContext? exerciseContext = null) =>
        new(sourceReference, step)
        {
            ExerciseId = exerciseContext?.Exercise.Id,
            ExerciseSourceReference = exerciseContext?.SourceReference,
            ExerciseSourceOffset = exerciseContext?.SourceOffset
        };

    private static WorkoutImportPrescriptionDraft CreateDurationDraft(
        RawDurationToken duration, string text, string? sourceReference)
    {
        var isTimeCap = duration.Role == DurationRole.TimeCap;
        var step = new WorkoutStepImportDraft
        {
            SourceText = text,
            StepKind = isTimeCap ? WorkoutStepKind.Instruction : WorkoutStepKind.Rest,
            RestSeconds = duration.Role == DurationRole.Rest ? duration.Seconds : null,
            MinimumRestSeconds = duration.Role == DurationRole.MinimumRest ? duration.Seconds : null,
            Confidence = 1
        };
        return new WorkoutImportPrescriptionDraft(sourceReference, step, isTimeCap ? duration.Seconds : null);
    }

    private static PrescriptionResult ResolveWork(
        RawWorkToken token, string text, string? sourceReference, ExercisePrescriptionContext? exerciseContext)
    {
        if (token.RequiresFollowingExercise && exerciseContext is null)
        {
            return new PrescriptionResult([], "The quantity is missing an adjacent resolved exercise.");
        }

        var (target, value, warning) = WorkoutWorkTargetResolver.Resolve(token.Work, exerciseContext);
        if (target is null)
        {
            return new PrescriptionResult([], warning);
        }

        var step = new WorkoutStepImportDraft
        {
            SourceText = text,
            ExerciseName = exerciseContext?.Exercise.Name,
            StepKind = WorkoutStepKind.Exercise,
            WorkTargetType = target,
            WorkValue = value,
            Scope = token.Work.Scope,
            SetCount = token.Work.SetCount,
            Confidence = 1
        };
        return new PrescriptionResult([CreateDraft(sourceReference, step, exerciseContext)], warning);
    }

    private static PrescriptionResult ResolveRepLoad(
        RawRepLoadToken token, string text, string? sourceReference, ExercisePrescriptionContext? exerciseContext,
        LoadUnit loadUnit)
    {
        var exercise = exerciseContext?.Exercise;
        if (exercise is null || !exercise.SupportsExternalLoad ||
            exercise.AllowedWorkTargetTypes?.Contains(WorkTargetType.Repetitions) != true)
        {
            return new PrescriptionResult([], "Compact rep/load notation requires an exercise that supports repetitions and external load.");
        }

        var drafts = token.Loads.Select(load => CreateDraft(sourceReference, new WorkoutStepImportDraft
        {
            SourceText = text,
            ExerciseName = exercise.Name,
            StepKind = WorkoutStepKind.Exercise,
            WorkTargetType = WorkTargetType.Repetitions,
            WorkValue = token.Reps,
            SetCount = token.Sets,
            LoadValue = load,
            LoadUnit = loadUnit,
            Confidence = 1
        }, exerciseContext)).ToList();
        return new PrescriptionResult(drafts, null);
    }

    private void LogMatch(WorkoutImportPrescriptionDraft draft, TextSpan span)
    {
        var step = draft.Step;
        logger.Log(nameof(WorkoutPrescriptionInterpreter),
            $"{draft.SourceReference} · matched {QuoteText(step.SourceText)} at offset {span.Start}, length {span.Length} · " +
            $"kind={step.StepKind}, sets={step.SetCount}, work={step.WorkValue}, target={step.WorkTargetType}, scope={step.Scope}, " +
            $"load={step.LoadValue}, load unit={step.LoadUnit}, rest seconds={step.RestSeconds}, " +
            $"minimum rest seconds={step.MinimumRestSeconds}, time cap seconds={draft.TimeCapSeconds}, notes={step.Notes}");
    }

    private void LogWarning(string? sourceReference, int offset, string text, string warning)
    {
        if (logger.Enabled)
        {
            logger.Log(nameof(WorkoutPrescriptionInterpreter),
                $"{sourceReference} · offset {offset} · {QuoteText(text)} · {warning}");
        }
    }
}

internal sealed record PrescriptionResult(IReadOnlyList<WorkoutImportPrescriptionDraft> Drafts, string? Warning);
