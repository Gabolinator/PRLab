namespace PRLab.Domain.Model.Value.Prescription.Intensity;

public sealed record TargetIntensityRange
{
    public decimal MinValue { get; init; }

    public decimal MaxValue { get; init; }

    private TargetIntensityRange()
    {
        // EF Core
    }

    private TargetIntensityRange(decimal minValue, decimal maxValue)
    {
        if (minValue <= 0)
        {
            throw new ArgumentException("Minimum intensity value must be greater than zero.", nameof(minValue));
        }

        if (maxValue <= 0)
        {
            throw new ArgumentException("Maximum intensity value must be greater than zero.", nameof(maxValue));
        }

        if (minValue > maxValue)
        {
            throw new ArgumentException("Minimum intensity value cannot be greater than maximum intensity value.");
        }

        MinValue = minValue;
        MaxValue = maxValue;
    }

    public static TargetIntensityRange New(decimal minValue, decimal maxValue)
    {
        return new TargetIntensityRange(minValue, maxValue);
    }
}