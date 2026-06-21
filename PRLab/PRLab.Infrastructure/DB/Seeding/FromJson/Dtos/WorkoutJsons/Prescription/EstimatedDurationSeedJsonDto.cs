using PRLab.Domain.Model.Value.Prescription.Common;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record EstimatedDurationSeedJsonDto
{
    public TimeSpan? Expected { get; init; }

    public TimeSpan? Minimum { get; init; }

    public TimeSpan? Maximum { get; init; }

    public static EstimatedDurationSeedJsonDto? FromEstimatedDuration(
        EstimatedDuration? estimatedDuration)
    {
        if (estimatedDuration is null)
        {
            return null;
        }

        return new EstimatedDurationSeedJsonDto
        {
            Expected = estimatedDuration.Expected,
            Minimum = estimatedDuration.Minimum,
            Maximum = estimatedDuration.Maximum
        };
    }

    public EstimatedDuration ToEstimatedDuration()
    {
        return EstimatedDuration.New(this.Expected, this.Minimum, this.Maximum);
    }
}