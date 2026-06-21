using PRLab.Domain.Model.Value.Enum.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Time;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record IntervalPrescriptionSeedJsonDto
{
    public TimeSpan Duration { get; init; }

    public IntervalScope Scope { get; init; }

    public bool StartsOnClock { get; init; }

    public static IntervalPrescriptionSeedJsonDto? FromIntervalPrescription(
        IntervalPrescription? intervalPrescription)
    {
        if (intervalPrescription is null)
        {
            return null;
        }

        return new IntervalPrescriptionSeedJsonDto
        {
            Duration = intervalPrescription.Duration,
            Scope = intervalPrescription.Scope,
            StartsOnClock = intervalPrescription.StartsOnClock
        };
    }
}