using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Policies;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons;

namespace PRLab.Infrastructure.DB.Seeding.Validation;

public static class WorkoutSeedValidator
{
    public static void Validate(WorkoutSeedJsonDto seedDto)
    {
        ArgumentNullException.ThrowIfNull(seedDto);

        var ownerUserId = seedDto.OwnerUserId.HasValue
            ? UserId.FromGuid(seedDto.OwnerUserId.Value)
            : (UserId?)null;

        try
        {
            DomainGuard.NotEmptyName(
                seedDto.Name,
                nameof(seedDto.Name));

            DomainGuard.ValidOptionalId(
                seedDto.Id,
                nameof(seedDto.Id));

            DataAccessPolicy.ValidateOwnership(
                seedDto.Origin,
                ownerUserId);

            DataAccessPolicy.ValidateVisibility(
                seedDto.Origin,
                seedDto.VisibilityScope);

            ValidateBlockSequences(seedDto);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' is invalid: {exception.Message}",
                exception);
        }

        ValidateSeedSpecificRules(seedDto);
    }

    private static void ValidateSeedSpecificRules(WorkoutSeedJsonDto seedDto)
    {
        if (seedDto.Id.HasValue &&
            seedDto.Origin != DataOrigin.BuiltIn)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' has a fixed Id but is not BuiltIn. " +
                "Only built-in seed data should use stable seed ids for now.");
        }
    }

    public static void ValidateBlockSequences(WorkoutSeedJsonDto seedDto)
    {
        var duplicateBlockSequence = seedDto.Blocks
            .GroupBy(block => block.Sequence)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateBlockSequence is not null)
        {
            throw new InvalidOperationException(
                $"Duplicate block sequence '{duplicateBlockSequence.Key}'.");
        }
    }
}