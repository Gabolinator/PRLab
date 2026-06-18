using PRLab.Domain.Model.Value.Enum.Prescription;

namespace PRLab.Domain.Model.Value.Prescription;

public sealed record WorkIntentPrescription
{
    public WorkIntent WorkIntent { get; init; }

    public TargetIntensity? TargetIntensity { get; init; }

    private WorkIntentPrescription()
    {
        // EF Core
    }

    private WorkIntentPrescription(
        WorkIntent workIntent,
        TargetIntensity? targetIntensity = null)
    {
        WorkIntent = workIntent;
        TargetIntensity = targetIntensity;
    }

    public static WorkIntentPrescription Normal()
    {
        return New(WorkIntent.Normal,null);
    }
    
    public static WorkIntentPrescription ForTime(
        TargetIntensity? targetIntensity = null)
    {
        return New(
            WorkIntent.ForTime,
            targetIntensity);
    }

    public static WorkIntentPrescription ForQuality(
        TargetIntensity? targetIntensity = null)
    {
        return New(
            WorkIntent.ForQuality,
            targetIntensity);
    }

    public static WorkIntentPrescription Technique(
        TargetIntensity? targetIntensity = null)
    {
        return New(
            WorkIntent.Technique,
            targetIntensity);
    }

    public static WorkIntentPrescription Practice(
        TargetIntensity? targetIntensity = null)
    {
        return New(
            WorkIntent.Practice,
            targetIntensity);
    }

    public static WorkIntentPrescription MaxEffort(
        TargetIntensity? targetIntensity = null)
    {
        return New(
            WorkIntent.MaxEffort,
            targetIntensity ?? TargetIntensity.RpeRange(9, 10));
    }

    public static WorkIntentPrescription Easy()
    {
        return New(
            WorkIntent.Normal,
            TargetIntensity.RpeRange(2, 4));
    }

    public static WorkIntentPrescription Moderate()
    {
        return New(
            WorkIntent.Normal,
            TargetIntensity.RpeRange(5, 7));
    }

    public static WorkIntentPrescription Hard()
    {
        return New(
            WorkIntent.Normal,
            TargetIntensity.RpeRange(8, 9));
    }
    
    public static WorkIntentPrescription Zone2()
    {
        return New(
            WorkIntent.Normal,
            TargetIntensity.Zone(2));
    }

    public static WorkIntentPrescription New(WorkIntent intent, TargetIntensity? targetIntensity)
    {
        return new WorkIntentPrescription(intent,
            targetIntensity);
    }
}