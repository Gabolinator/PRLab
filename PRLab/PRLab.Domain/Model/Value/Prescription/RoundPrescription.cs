namespace PRLab.Domain.Model.Value.Prescription;

public sealed record RoundPrescription
{
    public int NumRounds { get; init; }

    public TimeSpan? PrepareTime { get; init; }

    public RestTarget? RestBetweenRounds { get; init; }

    public RestTarget? RestAfterBlock { get; init; }
    
    public EstimatedDuration? EstimatedRoundDuration { get; init; } //per round or for the whole thing ? 

    private RoundPrescription()
    {
        // EF Core
    }

    private RoundPrescription(
        int numRounds,
        TimeSpan? prepareTime,
        RestTarget? restBetweenRounds,
        RestTarget? restAfterBlock,
        EstimatedDuration? estimatedRoundDuration)
    {
        if (numRounds < 1)
        {
            throw new ArgumentException("Number of rounds must be greater than zero.", nameof(numRounds));
        }

        if (prepareTime.HasValue && prepareTime.Value < TimeSpan.Zero)
        {
            throw new ArgumentException("Prepare time cannot be negative.", nameof(prepareTime));
        }

        NumRounds = numRounds;
        PrepareTime = prepareTime;
        RestBetweenRounds = restBetweenRounds;
        RestAfterBlock = restAfterBlock;
        EstimatedRoundDuration = estimatedRoundDuration;
    }

    public static RoundPrescription Once(
        TimeSpan? prepareTime = null,
        RestTarget? restAfterBlock = null,
        EstimatedDuration? estimatedDuration = null)
    {
        return new RoundPrescription(
            1,
            prepareTime,
            null,
            restAfterBlock,
            estimatedDuration);
    }

    public static RoundPrescription Rounds(
        int numRounds,
        TimeSpan? prepareTime = null,
        RestTarget? restBetweenRounds = null,
        RestTarget? restAfterBlock = null,
        EstimatedDuration? estimatedDuration = null)
    {
        return new RoundPrescription(
            numRounds,
            prepareTime,
            restBetweenRounds,
            restAfterBlock,
            estimatedDuration);
    }
}