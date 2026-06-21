using PRLab.Domain.Model.Value.Enum.Prescription.Intensity;
using PRLab.Domain.Model.Value.Prescription.Intensity;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record PaceTargetSeedJsonDto
{
    public PaceUnit Unit { get; init; }

    public TimeSpan Duration { get; init; }

    public static PaceTargetSeedJsonDto? FromPaceTarget(PaceTarget? paceTarget)
    {
        if (paceTarget is null)
        {
            return null;
        }

        return new PaceTargetSeedJsonDto
        {
            Unit = paceTarget.Unit,
            Duration = paceTarget.Duration
        };
    }
}