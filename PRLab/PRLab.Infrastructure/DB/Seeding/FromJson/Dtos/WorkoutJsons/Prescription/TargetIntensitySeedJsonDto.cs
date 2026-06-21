using PRLab.Domain.Model.Value.Enum.Prescription.Intensity;
using PRLab.Domain.Model.Value.Prescription.Intensity;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record TargetIntensitySeedJsonDto
{
    public IntensityMeasureType Type { get; init; }

    public decimal? Value { get; init; }

    public TargetIntensityRangeSeedJsonDto? Range { get; init; }

    public PaceTargetSeedJsonDto? PaceTarget { get; init; }

    public static TargetIntensitySeedJsonDto? FromTargetIntensity(
        TargetIntensity? targetIntensity)
    {
        if (targetIntensity is null)
        {
            return null;
        }

        return new TargetIntensitySeedJsonDto
        {
            Type = targetIntensity.Type,
            Value = targetIntensity.Value,
            Range = TargetIntensityRangeSeedJsonDto.FromRange(targetIntensity.Range),
            PaceTarget = PaceTargetSeedJsonDto.FromPaceTarget(targetIntensity.PaceTarget)
        };
    }
}