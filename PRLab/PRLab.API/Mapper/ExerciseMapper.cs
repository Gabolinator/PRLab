using PRLab.API.DTO.Exercise;
using PRLab.API.DTO.Exercise.Relation;
using PRLab.API.DTO.Movement;
using PRLab.API.Mapper;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;

namespace PRLab.API.Mapper;

public static class ExerciseMapper
{
    public static ExerciseGetDTO ToGetDTO(Exercise exercise)
    {
        return ToGetDTO(
            exercise,
            (LocalizationHelper.Language?)null
        );
    }

    public static ExerciseGetDTO ToGetDTO(
        Exercise exercise,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        return new ExerciseGetDTO(
            exercise.Id,
            exercise.Name,
            DescriptionMapper.ToGetDTO(exercise.Description, language),
            exercise.Blocks
                .OrderBy(block => block.Sequence)
                .Select(block => ToBlockGetDTO(block, language))
                .ToList()
        );
    }

    public static IReadOnlyCollection<ExerciseGetDTO> ToGetDTOs(
        IReadOnlyCollection<Exercise> exercises)
    {
        return ToGetDTOs(
            exercises,
            (LocalizationHelper.Language?)null
        );
    }

    public static IReadOnlyCollection<ExerciseGetDTO> ToGetDTOs(
        IReadOnlyCollection<Exercise> exercises,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(exercises);

        return exercises
            .Select(exercise => ToGetDTO(exercise, language))
            .ToList();
    }

    public static ExerciseSummaryDTO ToSummaryDTO(Exercise exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        return new ExerciseSummaryDTO(
            exercise.Id,
            exercise.Name
        );
    }

    public static Exercise ToEntity(
        ExercisePostDTO payload,
        User createdBy)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(createdBy);

        var description = payload.Descriptor is null
            ? Description.New(null)
            : DescriptionMapper.ToEntity(payload.Descriptor);

        var exercise = Exercise.NewUserCreated(
            payload.Name,
            description,
            createdBy
        );

        foreach (var block in payload.Blocks.OrderBy(block => block.Sequence))
        {
            exercise.AddBlock(
                movementId: block.MovementId,
                value: block.Target.Value,
                targetType: block.Target.TargetType,
                loadTarget: ToLoadTarget(block.LoadTarget),
                restBetweenReps: ToRestTarget(block.RestBetweenReps),
                transitionAfterBlock: ToRestTarget(block.TransitionAfterBlock),
                executionDetails: ToRepExecutionDetails(block.ExecutionDetails),
                changedBy: createdBy
            );
        }

        return exercise;
    }

    private static ExerciseBlockGetDTO ToBlockGetDTO(
        ExerciseBlock block,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(block);

        return new ExerciseBlockGetDTO(
            block.Id,
            block.Sequence,
            new MovementSummaryDTO(
                block.Movement.Id,
                block.Movement.Name
            ),
            ToWorkTargetGetDTO(block.Target),
            ToLoadTargetGetDTO(block.LoadTarget),
            ToRestTargetGetDTO(block.RestBetweenReps),
            ToRestTargetGetDTO(block.TransitionAfterBlock),
            ToRepExecutionDetailsGetDTO(block.ExecutionDetails)
        );
    }

    private static WorkTargetGetDTO ToWorkTargetGetDTO(WorkTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        return new WorkTargetGetDTO(
            target.Value,
            target.TargetType
        );
    }

    private static LoadTargetGetDTO ToLoadTargetGetDTO(LoadTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        return new LoadTargetGetDTO(
            target.Value,
            target.Type,
            target.Unit
        );
    }

    private static RestTargetGetDTO ToRestTargetGetDTO(RestTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        return new RestTargetGetDTO(
            target.Seconds
        );
    }

    private static RepExecutionDetailsGetDTO ToRepExecutionDetailsGetDTO(
        RepExecutionDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);

        return new RepExecutionDetailsGetDTO(
            details.EccentricSeconds,
            details.BottomPauseSeconds,
            details.ConcentricSeconds,
            details.TopPauseSeconds,
            details.EccentricIntent,
            details.BottomIntent,
            details.ConcentricIntent,
            details.TopIntent,
            details.Intent
        );
    }

    public static WorkTarget ToWorkTarget(WorkTargetPostDTO payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return WorkTarget.New(
            payload.Value,
            payload.TargetType
        );
    }

    public static WorkTarget ToWorkTarget(WorkTargetPutDTO payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return WorkTarget.New(
            payload.Value,
            payload.TargetType
        );
    }

    public static LoadTarget? ToLoadTarget(LoadTargetPostDTO? payload)
    {
        if (payload is null)
        {
            return null;
        }

        return ToLoadTarget(
            payload.Value,
            payload.Type,
            payload.Unit
        );
    }

    public static LoadTarget? ToLoadTarget(LoadTargetPutDTO? payload)
    {
        if (payload is null)
        {
            return null;
        }

        return ToLoadTarget(
            payload.Value,
            payload.Type,
            payload.Unit
        );
    }

    public static RestTarget? ToRestTarget(RestTargetPostDTO? payload)
    {
        if (payload is null)
        {
            return null;
        }

        return ToRestTarget(payload.Seconds);
    }

    public static RestTarget? ToRestTarget(RestTargetPutDTO? payload)
    {
        if (payload is null)
        {
            return null;
        }

        return ToRestTarget(payload.Seconds);
    }

    public static RepExecutionDetails? ToRepExecutionDetails(
        RepExecutionDetailsPostDTO? payload)
    {
        if (payload is null)
        {
            return null;
        }

        return RepExecutionDetails.New(
            payload.EccentricSeconds,
            payload.BottomPauseSeconds,
            payload.ConcentricSeconds,
            payload.TopPauseSeconds,
            payload.EccentricIntent,
            payload.BottomIntent,
            payload.ConcentricIntent,
            payload.TopIntent,
            payload.Intent
        );
    }

    public static RepExecutionDetails? ToRepExecutionDetails(
        RepExecutionDetailsPutDTO? payload)
    {
        if (payload is null)
        {
            return null;
        }

        return RepExecutionDetails.New(
            payload.EccentricSeconds,
            payload.BottomPauseSeconds,
            payload.ConcentricSeconds,
            payload.TopPauseSeconds,
            payload.EccentricIntent,
            payload.BottomIntent,
            payload.ConcentricIntent,
            payload.TopIntent,
            payload.Intent
        );
    }

    private static LoadTarget ToLoadTarget(
        decimal? value,
        DomainEnum.LoadTargetType type,
        DomainEnum.LoadUnit? unit)
    {
        return type switch
        {
            DomainEnum.LoadTargetType.None => LoadTarget.None(),

            DomainEnum.LoadTargetType.BodyWeight => LoadTarget.BodyWeight(),

            DomainEnum.LoadTargetType.ExternalLoad => LoadTarget.ExternalLoad(
                RequireLoadValue(value, type),
                RequireLoadUnit(unit, type)
            ),

            DomainEnum.LoadTargetType.AddedBodyWeightLoad => LoadTarget.AddedBodyWeightLoad(
                RequireLoadValue(value, type),
                RequireLoadUnit(unit, type)
            ),

            DomainEnum.LoadTargetType.AssistedBodyWeight => LoadTarget.AssistedBodyWeight(
                RequireLoadValue(value, type),
                RequireLoadUnit(unit, type)
            ),

            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unsupported load target type.")
        };
    }

    private static RestTarget ToRestTarget(int? seconds)
    {
        if (seconds is null)
        {
            return RestTarget.None();
        }

        return RestTarget.SecondsDuration(seconds.Value);
    }

    private static decimal RequireLoadValue(
        decimal? value,
        DomainEnum.LoadTargetType type)
    {
        if (!value.HasValue)
        {
            throw new ArgumentException($"Load value is required for {type}.");
        }

        return value.Value;
    }

    private static DomainEnum.LoadUnit RequireLoadUnit(
        DomainEnum.LoadUnit? unit,
        DomainEnum.LoadTargetType type)
    {
        if (!unit.HasValue)
        {
            throw new ArgumentException($"Load unit is required for {type}.");
        }

        return unit.Value;
    }
}