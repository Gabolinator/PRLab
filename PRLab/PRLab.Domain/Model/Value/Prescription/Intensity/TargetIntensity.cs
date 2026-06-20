using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Intensity;

namespace PRLab.Domain.Model.Value.Prescription.Intensity;

public sealed record TargetIntensity
{
    public IntensityMeasureType Type { get; init; }

    public decimal? Value { get; init; }

    public TargetIntensityRange? Range { get; init; }

    public PaceTarget? PaceTarget { get; init; }

    private TargetIntensity()
    {
        // EF Core
    }

    private TargetIntensity(
        IntensityMeasureType type,
        decimal? value,
        TargetIntensityRange? range,
        PaceTarget? paceTarget)
    {
        ValidateOrThrow(
            type,
            value,
            range,
            paceTarget);

        Type = type;
        Value = value;
        Range = range;
        PaceTarget = paceTarget;
    }

    public static TargetIntensity Rpe(decimal value)
    {
        return NewValue(IntensityMeasureType.Rpe, value);
    }

    public static TargetIntensity RpeRange(decimal minValue, decimal maxValue)
    {
        return NewRange(IntensityMeasureType.Rpe, minValue, maxValue);
    }

    public static TargetIntensity HeartRate(decimal bpm)
    {
        return NewValue(IntensityMeasureType.HeartRate, bpm);
    }

    public static TargetIntensity HeartRateRange(decimal minBpm, decimal maxBpm)
    {
        return NewRange(IntensityMeasureType.HeartRate, minBpm, maxBpm);
    }

    public static TargetIntensity PercentMaxHeartRate(decimal percent)
    {
        return NewValue(IntensityMeasureType.PercentMaxHeartRate, percent);
    }

    public static TargetIntensity PercentMaxHeartRateRange(decimal minPercent, decimal maxPercent)
    {
        return NewRange(IntensityMeasureType.PercentMaxHeartRate, minPercent, maxPercent);
    }

    public static TargetIntensity RepsInTheTank(decimal reps)
    {
        return NewValue(IntensityMeasureType.RepsInTheTank, reps);
    }

    public static TargetIntensity Pace(PaceTarget paceTarget)
    {
        ArgumentNullException.ThrowIfNull(paceTarget);

        return NewPace(
            IntensityMeasureType.Pace,
            paceTarget);
    }

    public static TargetIntensity PacePerKilometer(TimeSpan pace)
    {
        return Pace(PaceTarget.PerKilometer(pace));
    }

    public static TargetIntensity PacePerMile(TimeSpan pace)
    {
        return Pace(PaceTarget.PerMile(pace));
    }

    public static TargetIntensity SplitPer500Meters(TimeSpan split)
    {
        return NewPace(
            IntensityMeasureType.SplitTime,
            PaceTarget.Per500Meters(split));
    }

    public static TargetIntensity Watts(decimal value)
    {
        return NewValue(IntensityMeasureType.Watts, value);
    }

    public static TargetIntensity WattsRange(decimal minValue, decimal maxValue)
    {
        return NewRange(IntensityMeasureType.Watts, minValue, maxValue);
    }

    public static TargetIntensity Zone(decimal zone)
    {
        return NewValue(IntensityMeasureType.Zone, zone);
    }

    private static TargetIntensity NewValue(
        IntensityMeasureType type,
        decimal value)
    {
        ValidateIntensityValue(type, value);

        return new TargetIntensity(
            type,
            value,
            null,
            null);
    }

    private static TargetIntensity NewRange(
        IntensityMeasureType type,
        decimal minValue,
        decimal maxValue)
    {
        ValidateIntensityRange(type, minValue, maxValue);

        return new TargetIntensity(
            type,
            null,
            TargetIntensityRange.New(minValue, maxValue),
            null);
    }

    private static TargetIntensity NewPace(
        IntensityMeasureType type,
        PaceTarget paceTarget)
    {
        return new TargetIntensity(
            type,
            null,
            null,
            paceTarget);
    }

    private static void ValidateOrThrow(
        IntensityMeasureType type,
        decimal? value,
        TargetIntensityRange? range,
        PaceTarget? paceTarget)
    {
        var providedCount = 0;

        if (value is not null)
        {
            providedCount++;
        }

        if (range is not null)
        {
            providedCount++;
        }

        if (paceTarget is not null)
        {
            providedCount++;
        }

        if (providedCount == 0)
        {
            throw new ArgumentException("Target intensity must have a value, range, or pace target.");
        }

        if (providedCount > 1)
        {
            throw new ArgumentException("Target intensity cannot have more than one target shape.");
        }

        if (paceTarget is not null && !SupportsPaceTarget(type))
        {
            throw new ArgumentException($"{type} intensity cannot use a pace target.", nameof(paceTarget));
        }

        if (SupportsPaceTarget(type) && paceTarget is null)
        {
            throw new ArgumentException($"{type} intensity requires a pace target.", nameof(paceTarget));
        }
    }

    private static bool SupportsPaceTarget(IntensityMeasureType type)
    {
        return type is IntensityMeasureType.Pace or IntensityMeasureType.SplitTime;
    }

    private static void ValidateIntensityRange(
        IntensityMeasureType type,
        decimal minValue,
        decimal maxValue)
    {
        if (SupportsPaceTarget(type))
        {
            throw new ArgumentException($"{type} intensity should use PaceTarget instead of decimal range.");
        }

        if (minValue > maxValue)
        {
            throw new ArgumentException("Minimum intensity cannot be greater than maximum intensity.");
        }

        ValidateIntensityValue(type, minValue);
        ValidateIntensityValue(type, maxValue);
    }

    private static void ValidateIntensityValue(
        IntensityMeasureType type,
        decimal value)
    {
        if (SupportsPaceTarget(type))
        {
            throw new ArgumentException($"{type} intensity should use PaceTarget instead of decimal value.");
        }

        if (value < 0)
        {
            throw new ArgumentException("Intensity value cannot be negative.", nameof(value));
        }

        switch (type)
        {
            case IntensityMeasureType.Rpe:
                if (value > 10)
                {
                    throw new ArgumentException("RPE must be between 0 and 10.");
                }
                break;

            case IntensityMeasureType.PercentMaxHeartRate:
                if (value > 100)
                {
                    throw new ArgumentException("Percent max heart rate must be between 0 and 100.");
                }
                break;

            case IntensityMeasureType.Zone:
                if (value < 1 || value > 5)
                {
                    throw new ArgumentException("Zone must be between 1 and 5.");
                }
                break;

            default:
                if (value <= 0)
                {
                    throw new ArgumentException("Intensity value must be greater than zero.");
                }
                break;
        }
    }
}