using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Structure;

public sealed record WorkoutBlockSegmentSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Sequence { get; init; }

    public WorkMode WorkMode { get; init; }

    public WorkIntentPrescriptionSeedJsonDto Intent { get; init; } = new();

    public WorkoutScoreType ScoreType { get; init; }

    public TimeConstraintSeedJsonDto? TimeConstraint { get; init; }

    public IntervalPrescriptionSeedJsonDto? IntervalPrescription { get; init; }

    public EstimatedDurationSeedJsonDto? EstimatedSegmentDuration { get; init; }

    public RestTargetSeedJsonDto? RestAfterStep { get; init; }

    public RestTargetSeedJsonDto? RestAfterSegment { get; init; }

    public IReadOnlyList<WorkoutBlockSegmentStepSeedJsonDto> Steps { get; init; } = [];

    public static WorkoutBlockSegmentSeedJsonDto FromSegment(
        WorkoutBlockSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);

        return new WorkoutBlockSegmentSeedJsonDto
        {
            Id = segment.Id.Value,
            Name = segment.Name,
            Sequence = segment.Sequence,
            WorkMode = segment.WorkMode,
            Intent = WorkIntentPrescriptionSeedJsonDto.FromWorkIntentPrescription(
                segment.Intent),
            ScoreType = segment.ScoreType,
            TimeConstraint = TimeConstraintSeedJsonDto.FromTimeConstraint(
                segment.TimeConstraint),
            IntervalPrescription = IntervalPrescriptionSeedJsonDto.FromIntervalPrescription(
                segment.IntervalPrescription),
            EstimatedSegmentDuration = EstimatedDurationSeedJsonDto.FromEstimatedDuration(
                segment.EstimatedSegmentDuration),
            RestAfterStep = RestTargetSeedJsonDto.FromRestTarget(segment.RestAfterStep),
            RestAfterSegment = RestTargetSeedJsonDto.FromRestTarget(segment.RestAfterSegment),
            Steps = segment.Steps
                .OrderBy(step => step.Sequence)
                .Select(WorkoutBlockSegmentStepSeedJsonDto.FromStep)
                .ToList()
        };
    }
}