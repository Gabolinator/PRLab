using PRLab.Domain.Model.Value.Enum.Prescription.Work;

namespace PRLab.Domain.Model.Value.Prescription.Work;

public sealed record WorkTarget
{
    public decimal Value { get; private set; }

    public WorkTargetType TargetType { get; private set; }

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
        Scope = ValidateScope(scope);
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

    public static WorkTarget ForReps(int reps)
    {
        return New(
            reps,
            WorkTargetType.Repetitions);
    }

    public static WorkTarget ForRepsPerSide(int reps)
    {
        return New(
            reps,
            WorkTargetType.Repetitions,
            WorkTargetScope.PerSide);
    }

    public static WorkTarget ForRepsLeft(int reps)
    {
        return New(
            reps,
            WorkTargetType.Repetitions,
            WorkTargetScope.Left);
    }

    public static WorkTarget ForRepsRight(int reps)
    {
        return New(
            reps,
            WorkTargetType.Repetitions,
            WorkTargetScope.Right);
    }

    public static WorkTarget ForDuration(int seconds)
    {
        return New(
            seconds,
            WorkTargetType.DurationSeconds);
    }

    public static WorkTarget ForDurationPerSide(int seconds)
    {
        return New(
            seconds,
            WorkTargetType.DurationSeconds,
            WorkTargetScope.PerSide);
    }

    public static WorkTarget ForDurationLeft(int seconds)
    {
        return New(
            seconds,
            WorkTargetType.DurationSeconds,
            WorkTargetScope.Left);
    }

    public static WorkTarget ForDurationRight(int seconds)
    {
        return New(
            seconds,
            WorkTargetType.DurationSeconds,
            WorkTargetScope.Right);
    }

    public static WorkTarget ForDistance(decimal meters)
    {
        return New(
            meters,
            WorkTargetType.DistanceMeters);
    }

    public static WorkTarget ForDistancePerSide(decimal meters)
    {
        return New(
            meters,
            WorkTargetType.DistanceMeters,
            WorkTargetScope.PerSide);
    }

    public static WorkTarget ForCalories(decimal calories)
    {
        return New(
            calories,
            WorkTargetType.Calories);
    }

    public static WorkTarget ForTimeUnderTension(int seconds)
    {
        return New(
            seconds,
            WorkTargetType.TimeUnderTensionSeconds);
    }

    public static WorkTarget ForTimeUnderTensionPerSide(int seconds)
    {
        return New(
            seconds,
            WorkTargetType.TimeUnderTensionSeconds,
            WorkTargetScope.PerSide);
    }

    public static WorkTarget WithScope(
        WorkTarget target,
        WorkTargetScope scope)
    {
        ArgumentNullException.ThrowIfNull(target);

        return New(
            target.Value,
            target.TargetType,
            scope);
    }

    public WorkTarget AsTotal()
    {
        return this with
        {
            Scope = WorkTargetScope.Total
        };
    }

    public WorkTarget AsPerSide()
    {
        return this with
        {
            Scope = WorkTargetScope.PerSide
        };
    }

    public WorkTarget AsLeft()
    {
        return this with
        {
            Scope = WorkTargetScope.Left
        };
    }

    public WorkTarget AsRight()
    {
        return this with
        {
            Scope = WorkTargetScope.Right
        };
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
            throw new ArgumentException("Work target value must be greater than zero.", nameof(value));
        }

        return value;
    }

    private static WorkTargetScope ValidateScope(WorkTargetScope scope)
    {
        return scope switch
        {
            WorkTargetScope.Total => scope,
            WorkTargetScope.PerSide => scope,
            WorkTargetScope.Left => scope,
            WorkTargetScope.Right => scope,
            _ => throw new ArgumentOutOfRangeException(
                nameof(scope),
                scope,
                "Unsupported work target scope.")
        };
    }
}