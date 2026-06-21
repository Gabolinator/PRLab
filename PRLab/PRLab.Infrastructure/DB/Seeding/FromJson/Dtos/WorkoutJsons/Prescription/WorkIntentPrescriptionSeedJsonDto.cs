using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record WorkIntentPrescriptionSeedJsonDto
{
    public WorkIntent WorkIntent { get; init; }

    public TargetIntensitySeedJsonDto? TargetIntensity { get; init; }

    public static WorkIntentPrescriptionSeedJsonDto FromWorkIntentPrescription(
        WorkIntentPrescription prescription)
    {
        ArgumentNullException.ThrowIfNull(prescription);

        return new WorkIntentPrescriptionSeedJsonDto
        {
            WorkIntent = prescription.WorkIntent,
            TargetIntensity = TargetIntensitySeedJsonDto.FromTargetIntensity(
                prescription.TargetIntensity)
        };
    }

    public static WorkIntentPrescriptionSeedJsonDto? FromNullableWorkIntentPrescription(
        WorkIntentPrescription? prescription)
    {
        return prescription is null
            ? null
            : FromWorkIntentPrescription(prescription);
    }
}