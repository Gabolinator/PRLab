using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Repetition;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Exercise;

public sealed record ExerciseSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public DescriptionSeedJsonDto? Description { get; init; }

    public IReadOnlyList<ExerciseStepsSeedJsonDto> Steps { get; init; } = [];

    public DataOrigin Origin { get; init; } = DataOrigin.BuiltIn;

    public Guid? OwnerUserId { get; init; }

    public SeedAction Action { get; init; } = SeedAction.Ignore;

    public static ExerciseSeedJsonDto FromExercise(Domain.Model.Entity.Exercise exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        return new ExerciseSeedJsonDto
        {
            Id = exercise.Id.Value,
            Name = exercise.Name,
            NameKey = exercise.NameKey,
            Description = exercise.Description is null
                ? null
                : DescriptionSeedJsonDto.FromDescription(exercise.Description),
            Steps = exercise.Steps
                .OrderBy(block => block.Sequence)
                .Select(ExerciseStepsSeedJsonDto.FromExerciseStep)
                .ToList(),
            Origin = exercise.Ownership.Origin,
            OwnerUserId = exercise.Ownership.OwnerUserId?.Value,
            Action = SeedAction.Ignore,
        };
    }
}

public sealed record ExerciseStepsSeedJsonDto
{
    public Guid? Id { get; init; }

    public int Sequence { get; init; }

    public SeedEntityReferenceJsonDto Movement { get; init; } = new();

    public WorkTargetSeedJsonDto Target { get; init; } = new();

    public LoadTargetSeedJsonDto? LoadTarget { get; init; }

    public RestTargetSeedJsonDto? RestBetweenReps { get; init; }

    public RestTargetSeedJsonDto? TransitionAfterStep { get; init; }

    public RepExecutionDetailsSeedJsonDto? ExecutionDetails { get; init; }

    public static ExerciseStepsSeedJsonDto FromExerciseStep(ExerciseSteps exerciseSteps)
    {
        ArgumentNullException.ThrowIfNull(exerciseSteps);

        return new ExerciseStepsSeedJsonDto
        {
            Id = exerciseSteps.Id.Value,
            Sequence = exerciseSteps.Sequence,
            Movement = exerciseSteps.Movement is not null
                ? SeedEntityReferenceJsonDto.FromMovement(exerciseSteps.Movement)
                : new SeedEntityReferenceJsonDto
                {
                    Id = exerciseSteps.MovementId.Value,
                },
            Target = WorkTargetSeedJsonDto.FromWorkTarget(exerciseSteps.Target),
            LoadTarget = exerciseSteps.LoadTarget.Type == LoadTargetType.None
                ? null
                : LoadTargetSeedJsonDto.FromLoadTarget(exerciseSteps.LoadTarget),
            RestBetweenReps = exerciseSteps.RestBetweenReps.IsEmpty()
                ? null
                : RestTargetSeedJsonDto.FromRestTarget(exerciseSteps.RestBetweenReps),
            TransitionAfterStep = exerciseSteps.TransitionAfterStep.IsEmpty()
                ? null
                : RestTargetSeedJsonDto.FromRestTarget(exerciseSteps.TransitionAfterStep),
            ExecutionDetails = exerciseSteps.ExecutionDetails.IsEmpty()
                ? null
                : RepExecutionDetailsSeedJsonDto.FromRepExecutionDetails(exerciseSteps.ExecutionDetails),
        };
    }
}

public sealed record WorkTargetSeedJsonDto
{
    public decimal Value { get; init; }

    public WorkTargetType TargetType { get; init; }

    public static WorkTargetSeedJsonDto FromWorkTarget(WorkTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        return new WorkTargetSeedJsonDto
        {
            Value = target.Value,
            TargetType = target.TargetType,
        };
    }
}

public sealed record LoadTargetSeedJsonDto
{
    public decimal? Value { get; init; }

    public LoadTargetType Type { get; init; }

    public LoadUnit? Unit { get; init; }

    public static LoadTargetSeedJsonDto FromLoadTarget(LoadTarget loadTarget)
    {
        ArgumentNullException.ThrowIfNull(loadTarget);

        return new LoadTargetSeedJsonDto
        {
            Value = loadTarget.Value,
            Type = loadTarget.Type,
            Unit = loadTarget.Unit,
        };
    }
}

public sealed record RestTargetSeedJsonDto
{
    public int? Seconds { get; init; }

    public static RestTargetSeedJsonDto FromRestTarget(RestTarget restTarget)
    {
        ArgumentNullException.ThrowIfNull(restTarget);

        return new RestTargetSeedJsonDto
        {
            Seconds = restTarget.Seconds,
        };
    }
}

public sealed record RepExecutionDetailsSeedJsonDto
{
    public int? EccentricSeconds { get; init; }

    public int? BottomPauseSeconds { get; init; }

    public int? ConcentricSeconds { get; init; }

    public int? TopPauseSeconds { get; init; }

    public RepPhaseExecutionIntent? EccentricIntent { get; init; }

    public RepPhaseExecutionIntent? BottomIntent { get; init; }

    public RepPhaseExecutionIntent? ConcentricIntent { get; init; }

    public RepPhaseExecutionIntent? TopIntent { get; init; }

    public string? Intent { get; init; }

    public static RepExecutionDetailsSeedJsonDto FromRepExecutionDetails(
        RepExecutionDetails executionDetails)
    {
        ArgumentNullException.ThrowIfNull(executionDetails);

        return new RepExecutionDetailsSeedJsonDto
        {
            EccentricSeconds = executionDetails.EccentricSeconds,
            BottomPauseSeconds = executionDetails.BottomPauseSeconds,
            ConcentricSeconds = executionDetails.ConcentricSeconds,
            TopPauseSeconds = executionDetails.TopPauseSeconds,
            EccentricIntent = executionDetails.EccentricIntent,
            BottomIntent = executionDetails.BottomIntent,
            ConcentricIntent = executionDetails.ConcentricIntent,
            TopIntent = executionDetails.TopIntent,
            Intent = executionDetails.Intent,
        };
    }
}