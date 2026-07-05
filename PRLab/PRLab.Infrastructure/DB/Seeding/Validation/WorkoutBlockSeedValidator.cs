using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Policies;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Structure;

namespace PRLab.Infrastructure.DB.Seeding.Validation;

public static class WorkoutBlockSeedValidator
{
    public static void Validate(
        WorkoutBlockSeedJsonDto seedDto,
        string workoutName,
        DataOrigin workoutOrigin,
        int blockSequence)
    {
        ArgumentNullException.ThrowIfNull(seedDto);

        try
        {
            DomainGuard.NotEmptyName(
                seedDto.Name,
                nameof(seedDto.Name));

            DomainGuard.ValidOptionalId(
                seedDto.Id,
                nameof(seedDto.Id));

            DataAccessPolicy.ValidateVisibility(
                workoutOrigin,
                seedDto.VisibilityScope);

            ValidateSegmentSequences(seedDto);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockSequence}' is invalid: {exception.Message}",
                exception);
        }

        ValidateSeedSpecificRules(
            seedDto,
            workoutName,
            workoutOrigin);
    }

    private static void ValidateSeedSpecificRules(
        WorkoutBlockSeedJsonDto seedDto,
        string workoutName,
        DataOrigin workoutOrigin)
    {
        if (seedDto.Id.HasValue &&
            workoutOrigin != DataOrigin.BuiltIn)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{seedDto.Name}' has a fixed Id but the workout is not BuiltIn. " +
                "Only built-in seed data should use stable seed ids for now.");
        }

        if (seedDto.Segments.Count == 0)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{seedDto.Name}' must provide at least one segment.");
        }
    }

    private static void ValidateSegmentSequences(WorkoutBlockSeedJsonDto seedDto)
    {
        foreach (var segmentDto in seedDto.Segments)
        {
            if (segmentDto.Sequence < 1)
            {
                throw new InvalidOperationException(
                    $"Block '{seedDto.Name}' has segment with invalid sequence '{segmentDto.Sequence}'. Sequence must be greater than zero.");
            }
        }

        var duplicateSequences = seedDto.Segments
            .GroupBy(segment => segment.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new InvalidOperationException(
                $"Block '{seedDto.Name}' has duplicate segment sequence(s): {string.Join(", ", duplicateSequences)}.");
        }
    }
}