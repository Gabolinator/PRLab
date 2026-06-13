using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Enum.Prescription;
using PRLab.Domain.Value.Enum.System;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Exercise;

public sealed record ExerciseSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public DescriptionSeedJsonDto? Description { get; init; }

    public IReadOnlyList<ExerciseBlockSeedJsonDto> Blocks { get; init; } = [];

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
            Blocks = exercise.Blocks
                .OrderBy(block => block.Sequence)
                .Select(ExerciseBlockSeedJsonDto.FromExerciseBlock)
                .ToList(),
            Origin = exercise.Ownership.Origin,
            OwnerUserId = exercise.Ownership.OwnerUserId?.Value,
            Action = SeedAction.Ignore,
        };
    }
}

public sealed record ExerciseBlockSeedJsonDto
{
    public Guid? Id { get; init; }

    public int Sequence { get; init; }

    public SeedEntityReferenceJsonDto Movement { get; init; } = new();

    public WorkTargetSeedJsonDto Target { get; init; } = new();

    public LoadTargetSeedJsonDto? LoadTarget { get; init; }

    public RestTargetSeedJsonDto? RestBetweenReps { get; init; }

    public RestTargetSeedJsonDto? TransitionAfterBlock { get; init; }

    public RepExecutionDetailsSeedJsonDto? ExecutionDetails { get; init; }

    public static ExerciseBlockSeedJsonDto FromExerciseBlock(ExerciseBlock exerciseBlock)
    {
        ArgumentNullException.ThrowIfNull(exerciseBlock);

        return new ExerciseBlockSeedJsonDto
        {
            Id = exerciseBlock.Id.Value,
            Sequence = exerciseBlock.Sequence,
            Movement = exerciseBlock.Movement is not null
                ? SeedEntityReferenceJsonDto.FromMovement(exerciseBlock.Movement)
                : new SeedEntityReferenceJsonDto
                {
                    Id = exerciseBlock.MovementId.Value,
                },
            Target = WorkTargetSeedJsonDto.FromWorkTarget(exerciseBlock.Target),
            LoadTarget = exerciseBlock.LoadTarget.Type == LoadTargetType.None
                ? null
                : LoadTargetSeedJsonDto.FromLoadTarget(exerciseBlock.LoadTarget),
            RestBetweenReps = exerciseBlock.RestBetweenReps.IsEmpty()
                ? null
                : RestTargetSeedJsonDto.FromRestTarget(exerciseBlock.RestBetweenReps),
            TransitionAfterBlock = exerciseBlock.TransitionAfterBlock.IsEmpty()
                ? null
                : RestTargetSeedJsonDto.FromRestTarget(exerciseBlock.TransitionAfterBlock),
            ExecutionDetails = exerciseBlock.ExecutionDetails.IsEmpty()
                ? null
                : RepExecutionDetailsSeedJsonDto.FromRepExecutionDetails(exerciseBlock.ExecutionDetails),
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