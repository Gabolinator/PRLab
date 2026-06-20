using PRLab.Domain.Model.Value.Enum.Prescription.Intensity;

namespace PRLab.Domain.Model.Value.Prescription.Intensity;

public sealed record PaceTarget
{
    public PaceUnit Unit { get; init; }

    public TimeSpan Duration { get; init; }

    private PaceTarget()
    {
        // EF Core
    }

    private PaceTarget(
        PaceUnit unit,
        TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Pace duration must be greater than zero.", nameof(duration));
        }

        Unit = unit;
        Duration = duration;
    }

    public static PaceTarget PerKilometer(TimeSpan duration)
    {
        return new PaceTarget(
            PaceUnit.PerKilometer,
            duration);
    }

    public static PaceTarget PerMile(TimeSpan duration)
    {
        return new PaceTarget(
            PaceUnit.PerMile,
            duration);
    }

    public static PaceTarget Per500Meters(TimeSpan duration)
    {
        return new PaceTarget(
            PaceUnit.Per500Meters,
            duration);
    }
}