namespace PRLab.Domain.Model.Value.Prescription.Common;

public sealed record EstimatedDuration
{
    public TimeSpan Expected { get; init; }

    public TimeSpan? Minimum { get; init; }

    public TimeSpan? Maximum { get; init; }

    private EstimatedDuration()
    {
        // EF Core
    }

    private EstimatedDuration(
        TimeSpan expected,
        TimeSpan? minimum = null,
        TimeSpan? maximum = null)
    {
        if (expected <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Estimated duration must be greater than zero.",
                nameof(expected));
        }

        if (minimum.HasValue && minimum.Value <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Minimum estimated duration must be greater than zero.",
                nameof(minimum));
        }

        if (maximum.HasValue && maximum.Value <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Maximum estimated duration must be greater than zero.",
                nameof(maximum));
        }

        if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
        {
            throw new ArgumentException(
                "Minimum estimated duration cannot be greater than maximum estimated duration.");
        }

        if (minimum.HasValue && expected < minimum.Value)
        {
            throw new ArgumentException(
                "Expected estimated duration cannot be lower than minimum estimated duration.",
                nameof(expected));
        }

        if (maximum.HasValue && expected > maximum.Value)
        {
            throw new ArgumentException(
                "Expected estimated duration cannot be greater than maximum estimated duration.",
                nameof(expected));
        }

        Expected = expected;
        Minimum = minimum;
        Maximum = maximum;
    }

    public static EstimatedDuration Minutes(float minutes)
    {
        return From(TimeSpan.FromMinutes(minutes));
    }
    
    public static EstimatedDuration Seconds(int seconds)
    {
        return From(TimeSpan.FromSeconds(seconds));
    }

    public static EstimatedDuration MinutesWithMinMax(
        int expected,
        int? minimum = null,
        int? maximum = null)
    {
        return new EstimatedDuration(
            TimeSpan.FromMinutes(expected),
            minimum.HasValue ? TimeSpan.FromMinutes(minimum.Value) : null,
            maximum.HasValue ? TimeSpan.FromMinutes(maximum.Value) : null);
    }

    public static EstimatedDuration From(TimeSpan duration)
    {
        return new EstimatedDuration(duration);
    }

    public static EstimatedDuration FromWithMinMax(
        TimeSpan expected,
        TimeSpan? minimum = null,
        TimeSpan? maximum = null)
    {
        return new EstimatedDuration(expected, minimum, maximum);
    }
}
