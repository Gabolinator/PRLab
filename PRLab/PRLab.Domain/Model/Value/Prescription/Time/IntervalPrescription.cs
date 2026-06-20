using PRLab.Domain.Model.Value.Enum.Prescription.Time;

namespace PRLab.Domain.Model.Value.Prescription.Time;

public sealed record IntervalPrescription
{
    public TimeSpan Duration { get; init; }

    public IntervalScope Scope { get; init; }

    public bool StartsOnClock { get; init; }

    public IntervalPrescription()
    {
        // EF Core
    }

    public IntervalPrescription(
        TimeSpan duration,
        IntervalScope scope,
        bool startsOnClock)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Interval duration must be greater than zero.", nameof(duration));
        }

        ValidateScope(scope);

        Duration = duration;
        Scope = scope;
        StartsOnClock = startsOnClock;
    }
    

    public static IntervalPrescription PerStep(
        TimeSpan duration,
        bool startsOnClock = true)
    {
        return new IntervalPrescription(
            duration,
            IntervalScope.PerStep,
            startsOnClock);
    }

    public static IntervalPrescription PerSegment(
        TimeSpan duration,
        bool startsOnClock = true)
    {
        return new IntervalPrescription(
            duration,
            IntervalScope.PerSegment,
            startsOnClock);
    }

    public static IntervalPrescription PerBlockRepeat(
        TimeSpan duration,
        bool startsOnClock = true)
    {
        return new IntervalPrescription(
            duration,
            IntervalScope.PerBlockRepeat,
            startsOnClock);
    }

    private static void ValidateScope(IntervalScope scope)
    {
        switch (scope)
        {
            case IntervalScope.PerStep:
            case IntervalScope.PerSegment:
            case IntervalScope.PerBlockRepeat:
                return;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(scope),
                    scope,
                    "Unsupported interval scope.");
        }
    }
}