using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Workout;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record WorkoutStepPrescriptionSeedJsonDto
{
    public WorkTargetSeedJsonDto? WorkTarget { get; init; }

    public LoadTargetSeedJsonDto? LoadTarget { get; init; }

    public RestTargetSeedJsonDto? RestAfterStep { get; init; }

    public TimeConstraintSeedJsonDto? TimeConstraint { get; init; }

    public EstimatedDurationSeedJsonDto? EstimatedStepDuration { get; init; }

    public WorkIntentPrescriptionSeedJsonDto? IntentOverride { get; init; }

    public WorkPartitionPrescriptionSeedJsonDto? Partition { get; init; }

    public LateralityExecution? SideExecution { get; init; }

    public string? Notes { get; init; }

    public static WorkoutStepPrescriptionSeedJsonDto? FromPrescription(
        WorkoutStepPrescription? prescription)
    {
        if (prescription is null)
        {
            return null;
        }

        return new WorkoutStepPrescriptionSeedJsonDto
        {
            WorkTarget = WorkTargetSeedJsonDto.FromWorkTarget(prescription.WorkTarget),
            LoadTarget = LoadTargetSeedJsonDto.FromLoadTarget(prescription.LoadTarget),
            RestAfterStep = RestTargetSeedJsonDto.FromRestTarget(prescription.RestAfterStep),
            TimeConstraint = TimeConstraintSeedJsonDto.FromTimeConstraint(
                prescription.TimeConstraint),
            EstimatedStepDuration = EstimatedDurationSeedJsonDto.FromEstimatedDuration(
                prescription.EstimatedStepDuration),
            IntentOverride = WorkIntentPrescriptionSeedJsonDto.FromNullableWorkIntentPrescription(
                prescription.IntentOverride),
            Partition = WorkPartitionPrescriptionSeedJsonDto.FromPartition(
                prescription.Partition),
            SideExecution = prescription.SideExecution,
            Notes = prescription.Notes
        };
    }
}