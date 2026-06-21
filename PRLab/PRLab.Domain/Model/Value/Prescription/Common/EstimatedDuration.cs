namespace PRLab.Domain.Model.Value.Prescription.Common;

public sealed record EstimatedDuration
{
    public TimeSpan? Expected { get; init; }

    public TimeSpan? Minimum { get; init; }

    public TimeSpan? Maximum { get; init; }

    private EstimatedDuration()
    {
        // EF Core
    }

    private EstimatedDuration(
        TimeSpan? expected,
        TimeSpan? minimum,
        TimeSpan? maximum)
    {
        if (expected.HasValue && expected.Value < TimeSpan.Zero)
        {
            throw new ArgumentException("Expected duration cannot be negative.", nameof(expected));
        }

        if (minimum.HasValue && minimum.Value < TimeSpan.Zero)
        {
            throw new ArgumentException("Minimum duration cannot be negative.", nameof(minimum));
        }

        if (maximum.HasValue && maximum.Value < TimeSpan.Zero)
        {
            throw new ArgumentException("Maximum duration cannot be negative.", nameof(maximum));
        }

        if (minimum.HasValue && maximum.HasValue && minimum.Value > maximum.Value)
        {
            throw new ArgumentException("Minimum duration cannot be greater than maximum duration.");
        }

        if (expected.HasValue && minimum.HasValue && expected.Value < minimum.Value)
        {
            throw new ArgumentException("Expected duration cannot be less than minimum duration.");
        }

        if (expected.HasValue && maximum.HasValue && expected.Value > maximum.Value)
        {
            throw new ArgumentException("Expected duration cannot be greater than maximum duration.");
        }

        Expected = expected;
        Minimum = minimum;
        Maximum = maximum;
    }

    public static EstimatedDuration New(
        TimeSpan? expected = null,
        TimeSpan? minimum = null,
        TimeSpan? maximum = null)
    {
        if (!expected.HasValue && !minimum.HasValue && !maximum.HasValue)
        {
            throw new ArgumentException(
                "Estimated duration must provide at least one value.");
        }

        return new EstimatedDuration(
            expected,
            minimum,
            maximum);
    }

    public static EstimatedDuration Seconds(int seconds)
    {
        return New(TimeSpan.FromSeconds(seconds));
    }

    public static EstimatedDuration Minutes(float minutes)
    {
        return New(TimeSpan.FromMinutes(minutes));
    }

    public static EstimatedDuration Range(
        TimeSpan minimum,
        TimeSpan maximum)
    {
        return New(
            expected: null,
            minimum: minimum,
            maximum: maximum);
    }
}