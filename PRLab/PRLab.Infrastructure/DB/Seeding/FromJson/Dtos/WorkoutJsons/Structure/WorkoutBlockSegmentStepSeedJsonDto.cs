using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Structure;

public sealed record WorkoutBlockSegmentStepSeedJsonDto
{
    public Guid? Id { get; init; }

    public int Sequence { get; init; }

    public WorkoutStepKind StepKind { get; init; }

    public SeedEntityReferenceJsonDto? Exercise { get; init; }

    public WorkoutStepPrescriptionSeedJsonDto? Prescription { get; init; }

    public RestTargetSeedJsonDto? Rest { get; init; }

    public string? Notes { get; init; }

    public static WorkoutBlockSegmentStepSeedJsonDto FromStep(
        WorkoutBlockSegmentStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        return new WorkoutBlockSegmentStepSeedJsonDto
        {
            Id = step.Id.Value,
            Sequence = step.Sequence,
            StepKind = step.StepKind,
            Exercise = step.Exercise is null
                ? null
                : new SeedEntityReferenceJsonDto
                {
                    Id = step.Exercise.Id.Value,
                    Name = step.Exercise.Name,
                    NameKey = step.Exercise.NameKey
                },
            Prescription = WorkoutStepPrescriptionSeedJsonDto.FromPrescription(
                step.Prescription),
            Rest = RestTargetSeedJsonDto.FromRestTarget(step.Rest),
            Notes = step.Notes
        };
    }
}