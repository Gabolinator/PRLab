namespace PRLab.Domain.Model.Value.Prescription;

public sealed record PaceTarget
{
    public PaceUnit Unit { get; init; }

    public decimal SecondsPerUnit { get; init; }

    private PaceTarget()
    {
        // EF Core
    }

    private PaceTarget(
        PaceUnit unit,
        decimal secondsPerUnit)
    {
        if (secondsPerUnit <= 0)
        {
            throw new ArgumentException("Pace must be greater than zero.", nameof(secondsPerUnit));
        }

        Unit = unit;
        SecondsPerUnit = secondsPerUnit;
    }

    public static PaceTarget PerKilometer(TimeSpan pace)
    {
        return new PaceTarget(
            PaceUnit.PerKilometer,
            (decimal)pace.TotalSeconds);
    }

    public static PaceTarget PerMile(TimeSpan pace)
    {
        return new PaceTarget(
            PaceUnit.PerMile,
            (decimal)pace.TotalSeconds);
    }

    public static PaceTarget Per500Meters(TimeSpan pace)
    {
        return new PaceTarget(
            PaceUnit.Per500Meters,
            (decimal)pace.TotalSeconds);
    }
}