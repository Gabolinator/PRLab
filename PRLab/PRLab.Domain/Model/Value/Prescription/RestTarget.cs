using PRLab.Domain.Model.Value.Enum.Prescription;

namespace PRLab.Domain.Model.Value.Prescription;

public sealed record RestTarget
{
    public RestPolicy Policy { get; init; }

    public int? Seconds { get; init; }

    public int? MinimumSeconds { get; init; }

    public int? MaximumSeconds { get; init; }

    private RestTarget()
    {
        // EF Core
    }

    private RestTarget(
        RestPolicy policy,
        int? seconds,
        int? minimumSeconds,
        int? maximumSeconds)
    {
        Validate(policy, seconds, minimumSeconds, maximumSeconds);

        Policy = policy;
        Seconds = seconds;
        MinimumSeconds = minimumSeconds;
        MaximumSeconds = maximumSeconds;
    }

    public static RestTarget None()
    {
        return new RestTarget(
            RestPolicy.None,
            null,
            null,
            null);
    }

    public static RestTarget Fixed(int seconds)
    {
        return new RestTarget(
            RestPolicy.Fixed,
            seconds,
            null,
            null);
    }

    public static RestTarget NoMoreThan(int seconds)
    {
        return new RestTarget(
            RestPolicy.NoMoreThan,
            seconds,
            null,
            null);
    }

    public static RestTarget AtLeast(int seconds)
    {
        return new RestTarget(
            RestPolicy.AtLeast,
            seconds,
            null,
            null);
    }

    public static RestTarget Range(int minimumSeconds, int maximumSeconds)
    {
        return new RestTarget(
            RestPolicy.Range,
            null,
            minimumSeconds,
            maximumSeconds);
    }

    public static RestTarget AsNeeded()
    {
        return new RestTarget(
            RestPolicy.AsNeeded,
            null,
            null,
            null);
    }

    public static RestTarget UntilRecovered()
    {
        return new RestTarget(
            RestPolicy.UntilRecovered,
            null,
            null,
            null);
    }

    public static RestTarget SecondsDuration(int seconds)
    {
        return Fixed(seconds);
    }
    
    public bool IsEmpty()
    {
        return Policy == RestPolicy.None;
    }

    private static void Validate(
        RestPolicy policy,
        int? seconds,
        int? minimumSeconds,
        int? maximumSeconds)
    {
        if (seconds.HasValue && seconds.Value < 0)
        {
            throw new ArgumentException("Rest seconds cannot be negative.", nameof(seconds));
        }

        if (minimumSeconds.HasValue && minimumSeconds.Value < 0)
        {
            throw new ArgumentException("Minimum rest seconds cannot be negative.", nameof(minimumSeconds));
        }

        if (maximumSeconds.HasValue && maximumSeconds.Value < 0)
        {
            throw new ArgumentException("Maximum rest seconds cannot be negative.", nameof(maximumSeconds));
        }

        if (minimumSeconds.HasValue && maximumSeconds.HasValue && minimumSeconds.Value > maximumSeconds.Value)
        {
            throw new ArgumentException("Minimum rest cannot be greater than maximum rest.");
        }

        switch (policy)
        {
            case RestPolicy.None:
            case RestPolicy.AsNeeded:
            case RestPolicy.UntilRecovered:
                if (seconds.HasValue || minimumSeconds.HasValue || maximumSeconds.HasValue)
                {
                    throw new ArgumentException($"{policy} rest should not define a duration.");
                }
                break;

            case RestPolicy.Fixed:
            case RestPolicy.NoMoreThan:
            case RestPolicy.AtLeast:
                if (!seconds.HasValue)
                {
                    throw new ArgumentException($"{policy} rest requires seconds.", nameof(seconds));
                }

                if (minimumSeconds.HasValue || maximumSeconds.HasValue)
                {
                    throw new ArgumentException($"{policy} rest should not define a range.");
                }
                break;

            case RestPolicy.Range:
                if (!minimumSeconds.HasValue || !maximumSeconds.HasValue)
                {
                    throw new ArgumentException("Rest range requires minimum and maximum seconds.");
                }

                if (seconds.HasValue)
                {
                    throw new ArgumentException("Rest range should not define fixed seconds.");
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(policy), policy, "Unsupported rest policy.");
        }
    }
}