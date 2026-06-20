using PRLab.Domain.Model.Value.Prescription.Intensity;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;

namespace PRLab.Domain.Model.Value.Prescription.Work;

public sealed record WorkRepeatPrescription
{
    public int Sequence { get; init; }

    public WorkTarget? WorkTarget { get; init; }

    public LoadTarget? LoadTarget { get; init; }

    public RestTarget? RestAfterRepeat { get; init; }

    public TargetIntensity? TargetIntensity { get; init; }

    public string? Notes { get; init; }
}