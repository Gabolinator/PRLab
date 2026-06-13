using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Enum.Prescription;
using PRLab.Domain.Value.Identifier;
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
        User seedUser)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(seedUser);

        if (seedDto.Blocks.Count == 0)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{seedDto.Name}' must provide at least one block.");
        }

        ValidateBlockSequences(seedDto);

        foreach (var blockDto in seedDto.Blocks.OrderBy(block => block.Sequence))
        {
            var movement = ResolveMovement(
                blockDto.Movement,
                catalog,
                seedDto.Name,
                blockDto.Sequence);

            var target = ToWorkTarget(
                blockDto.Target,
                seedDto.Name,
                blockDto.Sequence);

            var loadTarget = ToLoadTarget(
                blockDto.LoadTarget,
                seedDto.Name,
                blockDto.Sequence);

            var restBetweenReps = ToRestTarget(
                blockDto.RestBetweenReps,
                seedDto.Name,
                blockDto.Sequence,
                nameof(blockDto.RestBetweenReps));

            var transitionAfterBlock = ToRestTarget(
                blockDto.TransitionAfterBlock,
                seedDto.Name,
                blockDto.Sequence,
                nameof(blockDto.TransitionAfterBlock));

            var executionDetails = ToRepExecutionDetails(
                blockDto.ExecutionDetails);

            exercise.AddBlock(
                movementId: movement.Id,
                target: target,
                loadTarget: loadTarget,
                restBetweenReps: restBetweenReps,
                transitionAfterBlock: transitionAfterBlock,
                executionDetails: executionDetails,
                changedBy: seedUser,
                atSequence: blockDto.Sequence);
        }
    }

    private static void ValidateBlockSequences(
        ExerciseSeedJsonDto seedDto)
    {
        foreach (var blockDto in seedDto.Blocks)
        {
            if (blockDto.Sequence < 1)
            {
                throw new InvalidOperationException(
                    $"Exercise seed '{seedDto.Name}' has block with invalid sequence '{blockDto.Sequence}'. Sequence must be greater than zero.");
            }
        }

        var duplicateSequences = seedDto.Blocks
            .GroupBy(block => block.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{seedDto.Name}' has duplicate block sequence(s): {string.Join(", ", duplicateSequences)}.");
        }
    }

    private static Movement ResolveMovement(
        SeedEntityReferenceJsonDto reference,
        MovementSeedCatalog movementCatalog,
        string exerciseName,
        int blockSequence)
    {
        if (reference.Id.HasValue)
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
            $"Exercise seed '{exerciseName}' block '{blockSequence}' movement reference must provide Id, NameKey, or Name.");
    }

    private static WorkTarget ToWorkTarget(
        WorkTargetSeedJsonDto dto,
        string exerciseName,
        int blockSequence)
    {
        if (dto.TargetType == WorkTargetType.Unspecified)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' block '{blockSequence}' must provide a valid target type.");
        }

        if (dto.Value <= 0)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' block '{blockSequence}' must provide a target value greater than zero.");
        }

        return WorkTarget.New(
            dto.Value,
            dto.TargetType);
    }

    private static LoadTarget ToLoadTarget(
        LoadTargetSeedJsonDto? dto,
        string exerciseName,
        int blockSequence)
    {
        if (dto is null || dto.Type == LoadTargetType.None)
        {
            return LoadTarget.None();
        }

        return dto.Type switch
        {
            LoadTargetType.BodyWeight => LoadTarget.BodyWeight(),

            LoadTargetType.ExternalLoad => LoadTarget.ExternalLoad(
                RequireLoadValue(dto, exerciseName, blockSequence),
                RequireLoadUnit(dto, exerciseName, blockSequence)),

            LoadTargetType.AddedBodyWeightLoad => LoadTarget.AddedBodyWeightLoad(
                RequireLoadValue(dto, exerciseName, blockSequence),
                RequireLoadUnit(dto, exerciseName, blockSequence)),

            LoadTargetType.AssistedBodyWeight => LoadTarget.AssistedBodyWeight(
                RequireLoadValue(dto, exerciseName, blockSequence),
                RequireLoadUnit(dto, exerciseName, blockSequence)),

            _ => throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' block '{blockSequence}' has unsupported load target type '{dto.Type}'.")
        };
    }

    private static decimal RequireLoadValue(
        LoadTargetSeedJsonDto dto,
        string exerciseName,
        int blockSequence)
    {
        if (!dto.Value.HasValue)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' block '{blockSequence}' load target '{dto.Type}' must provide Value.");
        }

        return dto.Value.Value;
    }

    private static LoadUnit RequireLoadUnit(
        LoadTargetSeedJsonDto dto,
        string exerciseName,
        int blockSequence)
    {
        if (!dto.Unit.HasValue)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' block '{blockSequence}' load target '{dto.Type}' must provide Unit.");
        }

        return dto.Unit.Value;
    }

    private static RestTarget ToRestTarget(
        RestTargetSeedJsonDto? dto,
        string exerciseName,
        int blockSequence,
        string fieldName)
    {
        if (dto is null || dto.Seconds is null)
        {
            return RestTarget.None();
        }

        if (dto.Seconds.Value < 0)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{exerciseName}' block '{blockSequence}' has invalid {fieldName}. Seconds cannot be negative.");
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