using PRLab.Domain.Value.Enum.Prescription;

namespace PRLab.Domain.Value;

public sealed record WorkTarget
{
    public decimal Value { get; private set; }

    public WorkTargetType TargetType { get; private set; }

    private WorkTarget()
    {
        // EF Core
    }

    private WorkTarget(
        decimal value,
        WorkTargetType targetType)
    {
        Value = ValidateValue(value);
        TargetType = targetType;
    }

    public static WorkTarget New(
        decimal value,
        WorkTargetType targetType)
    {
        return new WorkTarget(
            value,
            targetType);
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

    private static decimal ValidateValue(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Work target value must be greater than zero.");
        }

        return value;
    }
}