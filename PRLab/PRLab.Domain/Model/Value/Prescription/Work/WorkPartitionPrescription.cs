using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Rest;

namespace PRLab.Domain.Model.Value.Prescription.Work;

public sealed record WorkPartitionPrescription
{
    public WorkPartitionStrategy Strategy { get; init; }

    public int? RepeatCount { get; init; }

    public RestTarget? RestBetweenRepeats { get; init; }

    public IReadOnlyList<WorkRepeatPrescription> RepeatDetails { get; init; } = [];

    private WorkPartitionPrescription()
    {
        // EF Core
    }

    private WorkPartitionPrescription(
        WorkPartitionStrategy strategy,
        int? repeatCount,
        RestTarget? restBetweenRepeats = null,
        IReadOnlyList<WorkRepeatPrescription>? repeatDetails = null)
    {
        var normalizedRepeatDetails = repeatDetails?
            .OrderBy(repeatDetail => repeatDetail.Sequence)
            .ToList() ?? [];

        ValidateOrThrow(
            strategy,
            repeatCount,
            restBetweenRepeats,
            normalizedRepeatDetails);

        Strategy = strategy;
        RepeatCount = repeatCount;
        RestBetweenRepeats = restBetweenRepeats;
        RepeatDetails = normalizedRepeatDetails;
    }

    public static WorkPartitionPrescription Repeated(
        int repeatCount,
        RestTarget? restBetweenRepeats = null)
    {
        return new WorkPartitionPrescription(
            WorkPartitionStrategy.Repeated,
            repeatCount,
            restBetweenRepeats);
    }

    public static WorkPartitionPrescription VariableRepeats(
        IReadOnlyList<WorkRepeatPrescription> repeatDetails,
        RestTarget? restBetweenRepeats = null)
    {
        ArgumentNullException.ThrowIfNull(repeatDetails);

        return new WorkPartitionPrescription(
            WorkPartitionStrategy.VariableRepeats,
            repeatDetails.Count,
            restBetweenRepeats,
            repeatDetails);
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

    private static void ValidateOrThrow(
        WorkPartitionStrategy strategy,
        int? repeatCount,
        RestTarget? restBetweenRepeats,
        IReadOnlyList<WorkRepeatPrescription> repeatDetails)
    {
        switch (strategy)
        {
            case WorkPartitionStrategy.Repeated:
                ValidateRepeated(
                    repeatCount,
                    restBetweenRepeats,
                    repeatDetails);
                break;

            case WorkPartitionStrategy.VariableRepeats:
                ValidateVariableRepeats(
                    repeatCount,
                    repeatDetails);
                break;

            case WorkPartitionStrategy.SplitAnyhow:
            case WorkPartitionStrategy.Unbroken:
                ValidateNoRepeatData(
                    strategy,
                    repeatCount,
                    restBetweenRepeats,
                    repeatDetails);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(strategy),
                    strategy,
                    "Unsupported work partition strategy.");
        }
    }

    private static void ValidateRepeated(
        int? repeatCount,
        RestTarget? restBetweenRepeats,
        IReadOnlyList<WorkRepeatPrescription> repeatDetails)
    {
        if (!repeatCount.HasValue || repeatCount.Value < 1)
        {
            throw new ArgumentException(
                "Repeated partition requires a repeat count greater than zero.",
                nameof(repeatCount));
        }

        if (repeatDetails.Count > 0)
        {
            throw new ArgumentException(
                "Repeated partition should not define repeat details.",
                nameof(repeatDetails));
        }

        if (repeatCount.Value == 1 && restBetweenRepeats is not null)
        {
            throw new ArgumentException(
                "Rest between repeats should only be set when repeat count is greater than one.",
                nameof(restBetweenRepeats));
        }
    }

    private static void ValidateVariableRepeats(
        int? repeatCount,
        IReadOnlyList<WorkRepeatPrescription> repeatDetails)
    {
        if (repeatDetails.Count == 0)
        {
            throw new ArgumentException(
                "Variable repeats require at least one repeat detail.",
                nameof(repeatDetails));
        }

        if (!repeatCount.HasValue)
        {
            throw new ArgumentException(
                "Variable repeats require a repeat count.",
                nameof(repeatCount));
        }

        if (repeatCount.Value != repeatDetails.Count)
        {
            throw new ArgumentException(
                "Variable repeat count must match the number of repeat details.",
                nameof(repeatCount));
        }

        var duplicateSequences = repeatDetails
            .GroupBy(repeatDetail => repeatDetail.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new ArgumentException(
                $"Variable repeat details contain duplicate sequences: {string.Join(", ", duplicateSequences)}.",
                nameof(repeatDetails));
        }
    }

    private static void ValidateNoRepeatData(
        WorkPartitionStrategy strategy,
        int? repeatCount,
        RestTarget? restBetweenRepeats,
        IReadOnlyList<WorkRepeatPrescription> repeatDetails)
    {
        if (repeatCount.HasValue)
        {
            throw new ArgumentException(
                $"{strategy} partition should not define a repeat count.",
                nameof(repeatCount));
        }

        if (restBetweenRepeats is not null)
        {
            throw new ArgumentException(
                $"{strategy} partition should not define rest between repeats.",
                nameof(restBetweenRepeats));
        }

        if (repeatDetails.Count > 0)
        {
            throw new ArgumentException(
                $"{strategy} partition should not define repeat details.",
                nameof(repeatDetails));
        }
    }
}