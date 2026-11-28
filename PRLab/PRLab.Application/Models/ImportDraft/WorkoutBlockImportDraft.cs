using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutBlockImportDraft
{
    public required string Name { get; init; }

    public WorkoutBlockType? BlockType { get; init; }

    public WorkoutTrainingIntensity? Intensity { get; init; }

    public int RepeatCount { get; init; } = 1;

    public int? RestBetweenRepeatsSeconds { get; init; }

    public IReadOnlyList<WorkoutSegmentImportDraft> Segments { get; init; } = [];
    
    public int? Order { get; init; }

    public static WorkoutBlockImportDraft CreateFromRow(WorkoutImportRowDraft row)
    {
        var workWindow = row.WorkWindows.Single();
        var resolvedExercises = row.ExerciseParts.Where(part => part.ExerciseId.HasValue);
        var steps =
            resolvedExercises
                .SelectMany(part => ResolveExerciseSteps(row, part))
                .ToList();

        var segment = new WorkoutSegmentImportDraft
        {
            Name = workWindow.SourceText,
            WorkMode = workWindow.Mode,
            WorkWindowSeconds = workWindow.DurationSeconds,
            Steps = steps
        };

        return new WorkoutBlockImportDraft
        {
            Name = workWindow.SourceText,
            Order = row.Order,
            BlockType = row.RowSemanticItems.OfType<WorkoutBlockTypeSemantic>().FirstOrDefault()?.Type,
            Intensity = row.RowSemanticItems.OfType<WorkoutIntensitySemantic>().FirstOrDefault()?.Intensity,
            Segments = [segment]
        };
    }

    private static IEnumerable<WorkoutStepImportDraft> ResolveExerciseSteps(
        WorkoutImportRowDraft row,
        WorkoutImportExercisePart part)
    {
        var steps =
            row.Prescriptions
                .Where(prescription =>
                    prescription.ExerciseId == part.ExerciseId &&
                    prescription.ExerciseSourceReference == part.SourceReference &&
                    prescription.ExerciseSourceOffset == part.SourceOffset)
                .Select(prescription => prescription.Step)
                .ToList();

        var workSteps = steps.Where(step => step.Work is not null).ToList();
        var loads = steps.Select(step => step.Load).OfType<WorkoutLoadImportDraft>().ToList();
        var rates = steps.Select(step => step.Rate).OfType<WorkoutRateImportDraft>().ToList();
        var pause =
            steps
                .Select(step => step.Execution?.Pause)
                .OfType<WorkoutPauseImportDraft>()
                .MaxBy(pause => pause.Position.HasValue);
        var rate = rates.Count == 1 ? rates[0] : null;
        var selfPaced = steps.Any(step => step.Cadence?.Type.HasFlag(WorkoutCadenceType.SelfPaced) == true);

        if (workSteps.Count > 1 && workSteps.All(step => step.Load is not null))
        {
            return workSteps.Select(step => step with
            {
                Attributes = part.Attributes,
                Rate = rate,
                Cadence = WorkoutCadenceImportDraft.Create(step.Cadence?.IntervalSeconds, selfPaced),
                Execution = WorkoutExecutionImportDraft.Create(step.Execution?.Hold, pause)
            });
        }

        var workStep = workSteps.Count == 1 ? workSteps[0] : null;
        var load = loads.Count == 1 ? loads[0] : null;

        return
        [
            new WorkoutStepImportDraft
            {
                SourceText = part.SourceText,
                ExerciseName = part.ExerciseName,
                StepKind = WorkoutStepKind.Exercise,
                Work = workStep?.Work,
                Load = load,
                Cadence = WorkoutCadenceImportDraft.Create(workStep?.Cadence?.IntervalSeconds, selfPaced),
                Execution = WorkoutExecutionImportDraft.Create(workStep?.Execution?.Hold, pause),
                Rate = rate,
                Attributes = part.Attributes,
                Confidence = 1
            }
        ];
    }
}
