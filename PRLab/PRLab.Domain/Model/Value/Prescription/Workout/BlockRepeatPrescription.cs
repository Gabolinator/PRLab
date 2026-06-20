using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Rest;

namespace PRLab.Domain.Model.Value.Prescription.Workout;

public sealed record BlockRepeatPrescription
{
    public int RepeatCount { get; init; }

    public TimeSpan? PrepareTime { get; init; }

    public RestTarget? RestBetweenRepeats { get; init; }

    public RestTarget? RestAfterBlock { get; init; }

    public EstimatedDuration? EstimatedRepeatDuration { get; init; }

    private BlockRepeatPrescription()
    {
        // EF Core
    }

    private BlockRepeatPrescription(
        int repeatCount,
        TimeSpan? prepareTime,
        RestTarget? restBetweenRepeats,
        RestTarget? restAfterBlock,
        EstimatedDuration? estimatedRepeatDuration)
    {
        if (repeatCount < 1)
        {
            throw new ArgumentException("Repeat count must be greater than zero.", nameof(repeatCount));
        }

        if (prepareTime.HasValue && prepareTime.Value < TimeSpan.Zero)
        {
            throw new ArgumentException("Prepare time cannot be negative.", nameof(prepareTime));
        }

        RepeatCount = repeatCount;
        PrepareTime = prepareTime;
        RestBetweenRepeats = restBetweenRepeats;
        RestAfterBlock = restAfterBlock;
        EstimatedRepeatDuration = estimatedRepeatDuration;
    }

    public static BlockRepeatPrescription Once(
        TimeSpan? prepareTime = null,
        RestTarget? restAfterBlock = null,
        EstimatedDuration? estimatedRepeatDuration = null)
    {
        return new BlockRepeatPrescription(
            1,
            prepareTime,
            null,
            restAfterBlock,
            estimatedRepeatDuration);
    }

    public static BlockRepeatPrescription Repeat(
        int repeatCount,
        TimeSpan? prepareTime = null,
        RestTarget? restBetweenRepeats = null,
        RestTarget? restAfterBlock = null,
        EstimatedDuration? estimatedRepeatDuration = null)
    {
        return new BlockRepeatPrescription(
            repeatCount,
            prepareTime,
            restBetweenRepeats,
            restAfterBlock,
            estimatedRepeatDuration);
    }

    public static BlockRepeatPrescription Rounds(
        int numRounds,
        TimeSpan? prepareTime = null,
        RestTarget? restBetweenRounds = null,
        RestTarget? restAfterBlock = null,
        EstimatedDuration? estimatedRoundDuration = null)
    {
        return Repeat(
            numRounds,
            prepareTime,
            restBetweenRounds,
            restAfterBlock,
            estimatedRoundDuration);
    }
}