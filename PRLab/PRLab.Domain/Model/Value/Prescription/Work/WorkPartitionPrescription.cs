using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;

namespace PRLab.Domain.Model.Value.Prescription.Work;

public sealed record WorkPartitionPrescription
{
    public WorkPartitionStrategy Strategy { get; init; }

    public int? RepeatCount { get; init; }
    
    public IReadOnlyList<WorkRepeatPrescription> RepeatDetails { get; init; } = [];

    private WorkPartitionPrescription()
    {
        // EF Core
    }

    private WorkPartitionPrescription(
        WorkPartitionStrategy strategy,
        int? repeatCount)
    {
        if (RequiresRepeatCount(strategy) && (!repeatCount.HasValue || repeatCount.Value < 1))
        {
            throw new ArgumentException("This partition strategy requires a repeat count.", nameof(repeatCount));
        }

        if (!RequiresRepeatCount(strategy) && repeatCount.HasValue)
        {
            throw new ArgumentException("Repeat count should only be set when the strategy requires it.", nameof(repeatCount));
        }

        Strategy = strategy;
        RepeatCount = repeatCount;
    }

    public static WorkPartitionPrescription Repeated(int repeatCount)
    {
        return new WorkPartitionPrescription(
            WorkPartitionStrategy.Repeated,
            repeatCount);
    }

    public static WorkPartitionPrescription SplitAnyhow()
    {
        return new WorkPartitionPrescription(
            WorkPartitionStrategy.SplitAnyhow,
            null);
    }

    public static WorkPartitionPrescription Unbroken()
    {
        return new WorkPartitionPrescription(
            WorkPartitionStrategy.Unbroken,
            null);
    }
    
    public static WorkPartitionPrescription VariableRepeats(
        IReadOnlyList<WorkRepeatPrescription> repeatDetails)
    {
        if (repeatDetails.Count == 0)
        {
            throw new ArgumentException("Variable repeats require at least one repeat detail.");
        }

        return new WorkPartitionPrescription(
            WorkPartitionStrategy.VariableRepeats,
            repeatDetails.Count,
            repeatDetails);
    }

    private static bool RequiresRepeatCount(WorkPartitionStrategy strategy)
    {
        return strategy == WorkPartitionStrategy.Repeated;
    }
}