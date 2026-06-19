using PRLab.Domain.Model.Value.Enum.Prescription;

namespace PRLab.Domain.Model.Value.Prescription;

public sealed record IntervalPrescription
{
    public TimeSpan Duration { get; init; }

    public IntervalScope Scope { get; init; }

    public bool StartsOnClock { get; init; }
}