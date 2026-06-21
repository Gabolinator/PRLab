using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record WorkPartitionPrescriptionSeedJsonDto
{
    public WorkPartitionStrategy Strategy { get; init; }

    public int? RepeatCount { get; init; }

    public RestTargetSeedJsonDto? RestBetweenRepeats { get; init; }

    public IReadOnlyList<WorkRepeatPrescriptionSeedJsonDto> RepeatDetails { get; init; } = [];

    public static WorkPartitionPrescriptionSeedJsonDto? FromPartition(
        WorkPartitionPrescription? partition)
    {
        if (partition is null)
        {
            return null;
        }

        return new WorkPartitionPrescriptionSeedJsonDto
        {
            Strategy = partition.Strategy,
            RepeatCount = partition.RepeatCount,
            RestBetweenRepeats = RestTargetSeedJsonDto.FromRestTarget(
                partition.RestBetweenRepeats),
            RepeatDetails = partition.RepeatDetails
                .OrderBy(repeat => repeat.Sequence)
                .Select(WorkRepeatPrescriptionSeedJsonDto.FromRepeat)
                .ToList()
        };
    }
}