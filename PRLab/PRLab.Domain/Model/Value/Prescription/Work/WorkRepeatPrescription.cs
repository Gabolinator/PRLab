using PRLab.Domain.Model.Value.Prescription.Intensity;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;

namespace PRLab.Domain.Model.Value.Prescription.Work;

public sealed record WorkRepeatPrescription
{
    public int Sequence { get; init; }

    public WorkTarget? WorkTarget { get; init; }

    public LoadTarget? LoadTarget { get; init; }

    public TargetIntensity? TargetIntensity { get; init; }

    public RestTarget? RestAfterRepeat { get; init; }

    public string? Notes { get; init; }

    private WorkRepeatPrescription()
    {
        // EF Core
    }

    private WorkRepeatPrescription(
        int sequence,
        WorkTarget? workTarget,
        LoadTarget? loadTarget,
        TargetIntensity? targetIntensity,
        RestTarget? restAfterRepeat,
        string? notes)
    {
        if (sequence < 1)
        {
            throw new ArgumentException("Repeat sequence must be greater than zero.", nameof(sequence));
        }

        if (workTarget is null &&
            loadTarget is null &&
            targetIntensity is null &&
            restAfterRepeat is null &&
            string.IsNullOrWhiteSpace(notes))
        {
            throw new ArgumentException("Repeat detail must define at least one prescription value.");
        }

        Sequence = sequence;
        WorkTarget = workTarget;
        LoadTarget = loadTarget;
        TargetIntensity = targetIntensity;
        RestAfterRepeat = restAfterRepeat;
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    public static WorkRepeatPrescription New(
        int sequence,
        WorkTarget? workTarget = null,
        LoadTarget? loadTarget = null,
        TargetIntensity? targetIntensity = null,
        RestTarget? restAfterRepeat = null,
        string? notes = null)
    {
        return new WorkRepeatPrescription(
            sequence,
            workTarget,
            loadTarget,
            targetIntensity,
            restAfterRepeat,
            notes);
    }
}