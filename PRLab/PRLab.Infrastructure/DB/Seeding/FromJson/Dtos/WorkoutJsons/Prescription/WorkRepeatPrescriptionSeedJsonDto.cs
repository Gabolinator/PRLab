using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record WorkRepeatPrescriptionSeedJsonDto
{
    public int Sequence { get; init; }

    public WorkTargetSeedJsonDto? WorkTarget { get; init; }

    public LoadTargetSeedJsonDto? LoadTarget { get; init; }

    public TargetIntensitySeedJsonDto? TargetIntensity { get; init; }

    public RestTargetSeedJsonDto? RestAfterRepeat { get; init; }

    public string? Notes { get; init; }

    public static WorkRepeatPrescriptionSeedJsonDto FromRepeat(
        WorkRepeatPrescription repeat)
    {
        ArgumentNullException.ThrowIfNull(repeat);

        return new WorkRepeatPrescriptionSeedJsonDto
        {
            Sequence = repeat.Sequence,
            WorkTarget = WorkTargetSeedJsonDto.FromWorkTarget(repeat.WorkTarget),
            LoadTarget = LoadTargetSeedJsonDto.FromLoadTarget(repeat.LoadTarget),
            TargetIntensity = TargetIntensitySeedJsonDto.FromTargetIntensity(
                repeat.TargetIntensity),
            RestAfterRepeat = RestTargetSeedJsonDto.FromRestTarget(repeat.RestAfterRepeat),
            Notes = repeat.Notes
        };
    }
}