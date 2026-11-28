using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal static class WorkoutWorkTargetResolver
{
    public static (WorkTargetType? Target, decimal Value, string? Warning) Resolve(
        RawWorkoutWork work, ExercisePrescriptionContext? context)
    {
        var explicitTarget = GetExplicitTarget(work.Unit);
        var exercise = context?.Exercise;
        var allowedTypes = exercise?.AllowedWorkTargetTypes;
        var hint = context?.Hint;
        var invalidHint = hint.HasValue && allowedTypes is not null && !allowedTypes.Contains(hint.Value);
        var target = explicitTarget ?? hint ?? GetDefaultTarget(exercise);
        if (target is null)
        {
            return (null, work.Value, "Work target could not be inferred from its exercise.");
        }

        if (target == WorkTargetType.Unspecified || allowedTypes is not null && !allowedTypes.Contains(target.Value))
        {
            var origin = explicitTarget.HasValue ? "Explicit work target" : "Prescription hint";
            return (null, work.Value, $"{origin} {target} is not allowed for {exercise?.Name ?? "this exercise"}.");
        }

        var warning = invalidHint ? $"Prescription hint {hint} is not allowed for {exercise!.Name}." : null;
        var multiplier = work.Unit switch
        {
            "km" => 1000,
            "'" or "′" => 60,
            _ when work.Unit.StartsWith("min", StringComparison.Ordinal) => 60,
            _ => 1
        };
        return (target, (decimal)work.Value * multiplier, warning);
    }

    private static WorkTargetType? GetExplicitTarget(string unit) =>
        unit switch
        {
            "" => null,
            "m" or "km" => WorkTargetType.DistanceMeters,
            "cal" => WorkTargetType.Calories,
            "rep" or "reps" => WorkTargetType.Repetitions,
            _ => WorkTargetType.DurationSeconds
        };

    private static WorkTargetType? GetDefaultTarget(WorkoutImportExerciseReference? exercise)
    {
        var allowedTypes = exercise?.AllowedWorkTargetTypes;
        if (allowedTypes?.Count == 1 && !allowedTypes.Contains(WorkTargetType.Unspecified))
        {
            return allowedTypes.Single();
        }

        var defaultTarget = exercise?.DefaultWorkTargetType;
        return defaultTarget is { } target && target != WorkTargetType.Unspecified &&
               (allowedTypes is null || allowedTypes.Contains(target)) ? target : null;
    }
}

internal sealed record RawWorkoutWork(int Value, string Unit, int? SetCount, WorkTargetScope Scope);

internal sealed record ExercisePrescriptionContext(
    WorkoutImportExerciseReference Exercise, WorkTargetType? Hint, string? SourceReference, int SourceOffset)
{
    public bool HasExternalLoadEquipment { get; init; }

    public bool SupportsRepLoad =>
        (Exercise.SupportsExternalLoad || HasExternalLoadEquipment) &&
        Exercise.AllowedWorkTargetTypes?.Contains(WorkTargetType.Repetitions) == true;
}
