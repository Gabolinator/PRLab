using PRLab.API.DTO.Workout.WorkoutBlockSegmentStep;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper.WorkoutMappers;

public static class WorkoutBlockSegmentStepMapper
{
    public static IReadOnlyCollection<WorkoutBlockSegmentStepGetDTO> ToGetDTOs(
        IReadOnlyCollection<WorkoutBlockSegmentStep> steps)
    {
        return ToGetDTOs(
            steps,
            (LocalizationHelper.Language?)null);
    }

    public static IReadOnlyCollection<WorkoutBlockSegmentStepGetDTO> ToGetDTOs(
        IReadOnlyCollection<WorkoutBlockSegmentStep> steps,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(steps);

        return steps
            .OrderBy(step => step.Sequence)
            .Select(step => ToGetDTO(step, language))
            .ToList();
    }

    public static WorkoutBlockSegmentStepGetDTO ToGetDTO(
        WorkoutBlockSegmentStep step)
    {
        return ToGetDTO(
            step,
            (LocalizationHelper.Language?)null);
    }

    public static WorkoutBlockSegmentStepGetDTO ToGetDTO(
        WorkoutBlockSegmentStep step,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(step);

        ValidateEntityState(step);

        return new WorkoutBlockSegmentStepGetDTO(
            step.Id,
            step.StepKind,
            step.Sequence,
            step.Exercise is null
                ? null
                : ExerciseMapper.ToGetDTO(
                    step.Exercise,
                    language),
            step.Prescription,
            step.Rest,
            step.Notes);
    }

    public static WorkoutBlockSegmentStep ToEntity(
        WorkoutBlockSegmentStepPostDTO stepPayload,
        WorkoutBlockSegmentId segmentId)
    {
        ArgumentNullException.ThrowIfNull(stepPayload);

        if (segmentId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Workout block segment id cannot be empty.",
                nameof(segmentId));
        }

        ValidatePayload(stepPayload);

        return stepPayload.StepKind switch
        {
            WorkoutStepKind.Exercise =>
                WorkoutBlockSegmentStep.NewExerciseStep(
                    segmentId: segmentId,
                    exerciseId: stepPayload.ExerciseId!.Value,
                    sequence: stepPayload.Sequence,
                    prescription: stepPayload.Prescription!,
                    notes: stepPayload.Notes),

            WorkoutStepKind.Rest =>
                WorkoutBlockSegmentStep.NewRestStep(
                    segmentId: segmentId,
                    rest: stepPayload.Rest!,
                    sequence: stepPayload.Sequence,
                    notes: stepPayload.Notes),

            WorkoutStepKind.Instruction =>
                WorkoutBlockSegmentStep.NewInstructionStep(
                    segmentId: segmentId,
                    notes: stepPayload.Notes!,
                    sequence: stepPayload.Sequence),

            _ => throw new ArgumentOutOfRangeException(
                nameof(stepPayload.StepKind),
                stepPayload.StepKind,
                "Unsupported workout step kind.")
        };
    }

    private static void ValidatePayload(
        WorkoutBlockSegmentStepPostDTO payload)
    {
        if (payload.Sequence < 1)
        {
            throw new ArgumentException(
                "Workout segment step sequence must be greater than zero.",
                nameof(payload));
        }

        if (!Enum.IsDefined(payload.StepKind))
        {
            throw new ArgumentException(
                $"Unsupported workout step kind '{payload.StepKind}'.",
                nameof(payload));
        }

        switch (payload.StepKind)
        {
            case WorkoutStepKind.Exercise:
                ValidateExercisePayload(payload);
                break;

            case WorkoutStepKind.Rest:
                ValidateRestPayload(payload);
                break;

            case WorkoutStepKind.Instruction:
                ValidateInstructionPayload(payload);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(payload.StepKind),
                    payload.StepKind,
                    "Unsupported workout step kind.");
        }
    }

    private static void ValidateExercisePayload(
        WorkoutBlockSegmentStepPostDTO payload)
    {
        if (!payload.ExerciseId.HasValue ||
            payload.ExerciseId.Value.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "An exercise step requires a non-empty exercise id.",
                nameof(payload));
        }

        if (payload.Prescription is null)
        {
            throw new ArgumentException(
                "An exercise step requires a workout prescription.",
                nameof(payload));
        }

        if (payload.Rest is not null)
        {
            throw new ArgumentException(
                "An exercise step cannot define a rest-step target. " +
                "Use Prescription.RestAfterStep instead.",
                nameof(payload));
        }
    }

    private static void ValidateRestPayload(
        WorkoutBlockSegmentStepPostDTO payload)
    {
        if (payload.Rest is null)
        {
            throw new ArgumentException(
                "A rest step requires a rest target.",
                nameof(payload));
        }

        if (payload.ExerciseId.HasValue)
        {
            throw new ArgumentException(
                "A rest step cannot reference an exercise.",
                nameof(payload));
        }

        if (payload.Prescription is not null)
        {
            throw new ArgumentException(
                "A rest step cannot define an exercise prescription.",
                nameof(payload));
        }
    }

    private static void ValidateInstructionPayload(
        WorkoutBlockSegmentStepPostDTO payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Notes))
        {
            throw new ArgumentException(
                "An instruction step requires notes.",
                nameof(payload));
        }

        if (payload.ExerciseId.HasValue)
        {
            throw new ArgumentException(
                "An instruction step cannot reference an exercise.",
                nameof(payload));
        }

        if (payload.Prescription is not null)
        {
            throw new ArgumentException(
                "An instruction step cannot define an exercise prescription.",
                nameof(payload));
        }

        if (payload.Rest is not null)
        {
            throw new ArgumentException(
                "An instruction step cannot define a rest target.",
                nameof(payload));
        }
    }

    private static void ValidateEntityState(
        WorkoutBlockSegmentStep step)
    {
        switch (step.StepKind)
        {
            case WorkoutStepKind.Exercise:
                if (step.ExerciseId is null)
                {
                    throw new InvalidOperationException(
                        $"Exercise step '{step.Id}' has no exercise id.");
                }

                if (step.Exercise is null)
                {
                    throw new InvalidOperationException(
                        $"Exercise step '{step.Id}' was loaded without its exercise.");
                }

                if (step.Prescription is null)
                {
                    throw new InvalidOperationException(
                        $"Exercise step '{step.Id}' has no prescription.");
                }

                break;

            case WorkoutStepKind.Rest:
                if (step.Rest is null)
                {
                    throw new InvalidOperationException(
                        $"Rest step '{step.Id}' has no rest target.");
                }

                break;

            case WorkoutStepKind.Instruction:
                if (string.IsNullOrWhiteSpace(step.Notes))
                {
                    throw new InvalidOperationException(
                        $"Instruction step '{step.Id}' has no notes.");
                }

                break;

            default:
                throw new InvalidOperationException(
                    $"Step '{step.Id}' has unsupported kind '{step.StepKind}'.");
        }
    }
}