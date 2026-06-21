using PRLab.Domain.Model.Value.Prescription.Workout;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record BlockRepeatPrescriptionSeedJsonDto
{
    public int RepeatCount { get; init; }

    public TimeSpan? PrepareTime { get; init; }

    public RestTargetSeedJsonDto? RestBetweenRepeats { get; init; }

    public RestTargetSeedJsonDto? RestAfterBlock { get; init; }

    public EstimatedDurationSeedJsonDto? EstimatedRepeatDuration { get; init; }

    public static BlockRepeatPrescriptionSeedJsonDto FromBlockRepeatPrescription(
        BlockRepeatPrescription prescription)
    {
        ArgumentNullException.ThrowIfNull(prescription);

        return new BlockRepeatPrescriptionSeedJsonDto
        {
            RepeatCount = prescription.RepeatCount,
            PrepareTime = prescription.PrepareTime,
            RestBetweenRepeats = RestTargetSeedJsonDto.FromRestTarget(
                prescription.RestBetweenRepeats),
            RestAfterBlock = RestTargetSeedJsonDto.FromRestTarget(
                prescription.RestAfterBlock),
            EstimatedRepeatDuration = EstimatedDurationSeedJsonDto.FromEstimatedDuration(
                prescription.EstimatedRepeatDuration)
        };
    }
}