using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutBlockImportDraft
{
    public required string Name { get; init; }

    public WorkoutBlockType? BlockType { get; init; }

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
            Segments = [segment]
        };
    }

    private static IEnumerable<WorkoutStepImportDraft> ResolveExerciseSteps(
        WorkoutImportRowDraft row,
        WorkoutImportExercisePart part)
    {
        var prescriptions =
            row.Prescriptions
                .Where(prescription =>
                    prescription.ExerciseId == part.ExerciseId &&
                    prescription.ExerciseSourceReference == part.SourceReference &&
                    prescription.ExerciseSourceOffset == part.SourceOffset)
                .ToList();

        var workTargets =
            prescriptions
                .Where(prescription => prescription.Step.WorkTargetType.HasValue)
                .ToList();
        var loadTargets =
            prescriptions
                .Where(prescription => prescription.Step.LoadValue.HasValue)
                .ToList();
        var paceTargets =
            prescriptions
                .Where(prescription => prescription.Step.CaloriesPerHour.HasValue)
                .ToList();
        var caloriesPerHour = paceTargets.Count == 1 ? paceTargets[0].Step.CaloriesPerHour : null;

        if (workTargets.Count > 1 && workTargets.All(prescription => prescription.Step.LoadValue.HasValue))
        {
            return workTargets.Select(prescription => prescription.Step with
            {
                Attributes = part.Attributes,
                CaloriesPerHour = caloriesPerHour
            });
        }

        var work = workTargets.Count == 1 ? workTargets[0].Step : null;
        var load = loadTargets.Count == 1 ? loadTargets[0].Step : null;

        return
        [
            new WorkoutStepImportDraft
            {
                SourceText = part.SourceText,
                ExerciseName = part.ExerciseName,
                StepKind = WorkoutStepKind.Exercise,
                WorkValue = work?.WorkValue,
                WorkTargetType = work?.WorkTargetType,
                Scope = work?.Scope ?? WorkTargetScope.Total,
                SetCount = work?.SetCount,
                SetCadenceSeconds = work?.SetCadenceSeconds,
                CaloriesPerHour = caloriesPerHour,
                LoadValue = load?.LoadValue,
                LoadUnit = load?.LoadUnit,
                Attributes = part.Attributes,
                Confidence = 1
            }
        ];
    }
}
