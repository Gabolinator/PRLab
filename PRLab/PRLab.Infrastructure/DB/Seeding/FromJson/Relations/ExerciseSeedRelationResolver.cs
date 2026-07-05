using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Work;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Exercise;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations;

public sealed class ExerciseSeedRelationResolver : IExerciseSeedRelationResolver
{
    public void ApplyRelations(
        Exercise exercise,
        ExerciseSeedJsonDto seedDto,
        MovementSeedCatalog catalog,
        SeedExecutionOptions options,
        User seedUser)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(seedUser);

        if (seedDto.Steps.Count == 0)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{seedDto.Name}' must provide at least one Step.");
        }

        ValidateStepSequences(seedDto);

        foreach (var stepDto in seedDto.Steps.OrderBy(step => step.Sequence))
        {
            var movement = ResolveMovement(
                options,
                stepDto.Movement,
                catalog,
                seedDto.Name,
                stepDto.Sequence);

            var target = ToWorkTarget(
                stepDto.Target,
                seedDto.Name,
                stepDto.Sequence);

            var loadTarget = ToLoadTarget(
                stepDto.LoadTarget,
                seedDto.Name,
                stepDto.Sequence);

            var restBetweenReps = ToRestTarget(
                stepDto.RestBetweenReps,
                seedDto.Name,
                stepDto.Sequence,
                nameof(stepDto.RestBetweenReps));

            var transitionAfterStep = ToRestTarget(
                stepDto.TransitionAfterStep,
                seedDto.Name,
                stepDto.Sequence,
                nameof(stepDto.TransitionAfterStep));

            var executionDetails = ToRepExecutionDetails(
                stepDto.ExecutionDetails);

            exercise.AddStep(
                movementId: movement.Id,
                target: target,
                loadTarget: loadTarget,
                restBetweenReps: restBetweenReps,
                transitionAfterStep: transitionAfterStep,
                executionDetails: executionDetails,
                changedBy: seedUser,
                atSequence: stepDto.Sequence);
        }
    }
    private static void ValidateStepSequences(
        ExerciseSeedJsonDto seedDto)
    {
        foreach (var stepDto in seedDto.Steps)
        {
            if (stepDto.Sequence < 1)
            {
                throw new InvalidOperationException(
                    $"Exercise seed '{seedDto.Name}' has Step with invalid sequence '{stepDto.Sequence}'. Sequence must be greater than zero.");
            }
        }
        
        var duplicateSequences = seedDto.Steps
            .GroupBy(step => step.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{seedDto.Name}' has duplicate Step sequence(s): {string.Join(", ", duplicateSequences)}.");
        }
    }

    private static Movement ResolveMovement(
        SeedExecutionOptions options,
        SeedEntityReferenceJsonDto reference,
        MovementSeedCatalog movementCatalog,
        string exerciseName,
        int stepSequence)
    {
        if (reference.Id.HasValue && !options.IgnoreReferenceIds)
        {
            return movementCatalog.GetRequiredById(
                MovementId.FromGuid(reference.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(reference.NameKey))
        {
            return movementCatalog.GetRequiredByNameKey(reference.NameKey);
        }

        if (!string.IsNullOrWhiteSpace(reference.Name))
        {
            return movementCatalog.GetRequiredByName(reference.Name);
        }

        throw new InvalidOperationException(
            $"Exercise seed '{exerciseName}' Step '{stepSequence}' movement reference must provide Id, NameKey, or Name.");
    }

    private static WorkTarget ToWorkTarget(
        WorkTargetSeedJsonDto dto,
        string exerciseName,
        int StepSequence)
    {
        if (dto.TargetType == WorkTargetType.Unspecified)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' Step '{StepSequence}' must provide a valid target type.");
        }

        if (dto.Value <= 0)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' Step '{StepSequence}' must provide a target value greater than zero.");
        }

        return WorkTarget.New(
            dto.Value,
            dto.TargetType);
    }

    private static LoadTarget ToLoadTarget(
        LoadTargetSeedJsonDto? dto,
        string exerciseName,
        int StepSequence)
    {
        if (dto is null || dto.Type == LoadTargetType.None)
        {
            return LoadTarget.None();
        }

        return dto.Type switch
        {
            LoadTargetType.BodyWeight => LoadTarget.BodyWeight(),

            LoadTargetType.ExternalLoad => LoadTarget.ExternalLoad(
                RequireLoadValue(dto, exerciseName, StepSequence),
                RequireLoadUnit(dto, exerciseName, StepSequence)),

            LoadTargetType.AddedBodyWeightLoad => LoadTarget.AddedBodyWeightLoad(
                RequireLoadValue(dto, exerciseName, StepSequence),
                RequireLoadUnit(dto, exerciseName, StepSequence)),

            LoadTargetType.AssistedBodyWeight => LoadTarget.AssistedBodyWeight(
                RequireLoadValue(dto, exerciseName, StepSequence),
                RequireLoadUnit(dto, exerciseName, StepSequence)),

            _ => throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' Step '{StepSequence}' has unsupported load target type '{dto.Type}'.")
        };
    }

    private static decimal RequireLoadValue(
        LoadTargetSeedJsonDto dto,
        string exerciseName,
        int StepSequence)
    {
        if (!dto.Value.HasValue)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' Step '{StepSequence}' load target '{dto.Type}' must provide Value.");
        }

        return dto.Value.Value;
    }

    private static LoadUnit RequireLoadUnit(
        LoadTargetSeedJsonDto dto,
        string exerciseName,
        int StepSequence)
    {
        if (!dto.Unit.HasValue)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' Step '{StepSequence}' load target '{dto.Type}' must provide Unit.");
        }

        return dto.Unit.Value;
    }

    private static RestTarget ToRestTarget(
        RestTargetSeedJsonDto? dto,
        string exerciseName,
        int StepSequence,
        string fieldName)
    {
        if (dto is null || dto.Seconds is null)
        {
            return RestTarget.None();
        }

        if (dto.Seconds.Value < 0)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' Step '{StepSequence}' has invalid {fieldName}. Seconds cannot be negative.");
        }

        return RestTarget.SecondsDuration(dto.Seconds.Value);
    }

    private static RepExecutionDetails ToRepExecutionDetails(
        RepExecutionDetailsSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return RepExecutionDetails.Empty();
        }

        return RepExecutionDetails.New(
            eccentricSeconds: dto.EccentricSeconds,
            bottomPauseSeconds: dto.BottomPauseSeconds,
            concentricSeconds: dto.ConcentricSeconds,
            topPauseSeconds: dto.TopPauseSeconds,
            eccentricIntent: dto.EccentricIntent,
            bottomIntent: dto.BottomIntent,
            concentricIntent: dto.ConcentricIntent,
            topIntent: dto.TopIntent,
            intent: dto.Intent);
    }
}