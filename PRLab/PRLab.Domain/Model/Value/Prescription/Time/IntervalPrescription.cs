using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Time;

namespace PRLab.Domain.Model.Value.Prescription.Time;

public sealed record IntervalPrescription
{
    public TimeSpan Duration { get; init; }

    public IntervalScope Scope { get; init; }

    public bool StartsOnClock { get; init; }
}