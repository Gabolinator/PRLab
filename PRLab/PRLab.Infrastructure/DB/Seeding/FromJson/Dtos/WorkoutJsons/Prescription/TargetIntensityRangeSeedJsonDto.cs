using PRLab.Domain.Model.Value.Prescription.Intensity;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record TargetIntensityRangeSeedJsonDto
{
    public decimal MinValue { get; init; }

    public decimal MaxValue { get; init; }

    public static TargetIntensityRangeSeedJsonDto? FromRange(
        TargetIntensityRange? range)
    {
        if (range is null)
        {
            return null;
        }

        return new TargetIntensityRangeSeedJsonDto
        {
            MinValue = range.MinValue,
            MaxValue = range.MaxValue
        };
    }
}