using PRLab.Domain.Model.Value.Enum.Prescription;

namespace PRLab.Domain.Model.Value.Prescription;

public sealed record TimeConstraint
{
    public TimeConstraintKind Kind { get; init; }

    public TimeSpan? Duration { get; init; }

    private TimeConstraint()
    {
        // EF Core
    }

    private TimeConstraint(TimeConstraintKind kind, TimeSpan? duration)
    {
        if (kind == TimeConstraintKind.RemainingSegmentTime)
        {
            if (duration is not null)
            {
                throw new ArgumentException("Remaining segment time should not have a duration.");
            }
        }
        else if (!duration.HasValue || duration.Value <= TimeSpan.Zero)
        {
            throw new ArgumentException("Duration must be greater than zero.", nameof(duration));
        }

        Kind = kind;
        Duration = duration;
    }

    public static TimeConstraint Target(TimeSpan duration)
    {
        return new TimeConstraint(TimeConstraintKind.Target, duration);
    }
    
    public static TimeConstraint Interval(TimeSpan duration)
    {
        return new TimeConstraint(TimeConstraintKind.Interval, duration);
    }

    public static TimeConstraint Cap(TimeSpan duration)
    {
        return new TimeConstraint(TimeConstraintKind.Cap, duration);
    }

    public static TimeConstraint Minimum(TimeSpan duration)
    {
        return new TimeConstraint(TimeConstraintKind.Minimum, duration);
    }

    public static TimeConstraint Window(TimeSpan duration)
    {
        return new TimeConstraint(TimeConstraintKind.Window, duration);
    }

    public static TimeConstraint RemainingSegmentTime()
    {
        return new TimeConstraint(TimeConstraintKind.RemainingSegmentTime, null);
    }
}