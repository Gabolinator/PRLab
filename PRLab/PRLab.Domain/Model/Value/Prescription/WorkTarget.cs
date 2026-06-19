using PRLab.Domain.Model.Value.Enum.Prescription;

namespace PRLab.Domain.Model.Value.Prescription;

public sealed record WorkTarget
{
    public decimal Value { get; private set; }

    public WorkTargetType TargetType { get; private set; }
    
    //todo include in model
    public WorkTargetScope Scope { get; private set; } = WorkTargetScope.Total;

    private WorkTarget()
    {
        // EF Core
    }

    private WorkTarget(
        decimal value,
        WorkTargetType targetType,
        WorkTargetScope scope)
    {
        Value = ValidateValue(value);
        TargetType = targetType;
        Scope = scope;
    }

    public static WorkTarget New(
        decimal value,
        WorkTargetType targetType,
        WorkTargetScope scope = WorkTargetScope.Total)
    {
        return new WorkTarget(
            value,
            targetType,
            scope);
    }


    public static WorkTarget ForReps(
        int reps)
    {
        return New(
            reps,
            WorkTargetType.Repetitions);
    }

    public static WorkTarget ForDuration(
        int seconds)
    {
        return New(
            seconds,
            WorkTargetType.DurationSeconds);
    }

    public static WorkTarget ForDistance(
        decimal meters)
    {
        return New(
            meters,
            WorkTargetType.DistanceMeters);
    }

    public static WorkTarget ForCalories(
        decimal calories)
    {
        return New(
            calories,
            WorkTargetType.Calories);
    }

    public static WorkTarget ForTimeUnderTension(
        int seconds)
    {
        return New(
            seconds,
            WorkTargetType.TimeUnderTensionSeconds);
    }
    
    public static WorkTarget ForRepsPerSide(int reps)
    {
        return New(
            reps,
            WorkTargetType.Repetitions,
            WorkTargetScope.PerSide);
    }

    
    public static WorkTarget ForDurationPerSide(int seconds)
    {
        return New(
            seconds,
            WorkTargetType.DurationSeconds,
            WorkTargetScope.PerSide);
    }
    
    public static WorkTarget FromDefaultWorkType(WorkTargetType targetType)
    {
        return targetType switch
        {
            WorkTargetType.Repetitions => ForReps(1),
            WorkTargetType.DurationSeconds => ForDuration(30),
            WorkTargetType.DistanceMeters => ForDistance(100),
            WorkTargetType.Calories => ForCalories(10),
            WorkTargetType.TimeUnderTensionSeconds => ForTimeUnderTension(30),
            _ => throw new ArgumentOutOfRangeException(
                nameof(targetType),
                targetType,
                "Unsupported default work target type.")
        };
    }

    private static decimal ValidateValue(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Work target value must be greater than zero.");
        }

        return value;
    }
}