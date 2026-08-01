using PRLab.API.DTO.Workout.WorkoutBlockSegmentStep;
using PRLab.API.DTO.Workout.WorkoutSegments;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper.WorkoutMappers;

public static class WorkoutBlockSegmentMapper
{
    public static IReadOnlyCollection<WorkoutBlockSegmentGetDTO> ToGetDTOs(
        IReadOnlyCollection<WorkoutBlockSegment> segments)
    {
        return ToGetDTOs(
            segments,
            (LocalizationHelper.Language?)null);
    }

    public static IReadOnlyCollection<WorkoutBlockSegmentGetDTO> ToGetDTOs(
        IReadOnlyCollection<WorkoutBlockSegment> segments,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(segments);

        return segments
            .OrderBy(segment => segment.Sequence)
            .Select(segment => ToGetDTO(segment, language))
            .ToList();
    }

    public static WorkoutBlockSegmentGetDTO ToGetDTO(
        WorkoutBlockSegment segment)
    {
        return ToGetDTO(
            segment,
            (LocalizationHelper.Language?)null);
    }

    public static WorkoutBlockSegmentGetDTO ToGetDTO(
        WorkoutBlockSegment segment,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(segment);

        return new WorkoutBlockSegmentGetDTO(
            segment.Id,
            segment.Name,
            segment.Sequence,
            segment.WorkMode,
            segment.Intent,
            segment.ScoreType,
            segment.TimeConstraint,
            segment.IntervalPrescription,
            segment.EstimatedSegmentDuration,
            segment.RestAfterStep,
            segment.RestAfterSegment,
            segment.Steps
                .OrderBy(step => step.Sequence)
                .Select(step =>
                    WorkoutBlockSegmentStepMapper.ToGetDTO(
                        step,
                        language))
                .ToList());
    }

    public static WorkoutBlockSegment ToEntity(
        WorkoutBlockSegmentPostDTO payload,
        WorkoutBlockId workoutBlockId)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (workoutBlockId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Workout block id cannot be empty.",
                nameof(workoutBlockId));
        }

        ValidateStepSequences(payload.Steps);

        var segment = WorkoutBlockSegment.New(
            workoutBlockId: workoutBlockId,
            name: payload.Name,
            sequence: payload.Sequence,
            workMode: payload.WorkMode,
            intent: payload.Intent,
            scoreType: payload.ScoreType,
            timeConstraint: payload.TimeConstraint,
            intervalPrescription: payload.IntervalPrescription,
            estimatedSegmentDuration: payload.EstimatedSegmentDuration,
            restAfterStep: payload.RestAfterStep,
            restAfterSegment: payload.RestAfterSegment);

        foreach (var stepPayload in payload.Steps
                     .OrderBy(step => step.Sequence))
        {
            var step = WorkoutBlockSegmentStepMapper.ToEntity(
                stepPayload,
                segment.Id);

            segment.AddStep(step);
        }

        return segment;
    }

    private static void ValidateStepSequences(
        IReadOnlyCollection<WorkoutBlockSegmentStepPostDTO> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        var invalidSequence = steps
            .FirstOrDefault(step => step.Sequence < 1);

        if (invalidSequence is not null)
        {
            throw new ArgumentException(
                "Workout segment step sequences must be greater than zero.",
                nameof(steps));
        }

        var duplicateSequences = steps
            .GroupBy(step => step.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(sequence => sequence)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new ArgumentException(
                $"Workout segment step sequences contain duplicate value(s): " +
                $"{string.Join(", ", duplicateSequences)}.",
                nameof(steps));
        }
    }
}