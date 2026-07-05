using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Policies;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement;

namespace PRLab.Infrastructure.DB.Seeding.Validation;

public static class MovementSeedValidator
{
    public static void Validate(MovementSeedJsonDto seedDto)
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

            ValidateWorkTargetTypes(seedDto);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Movement seed '{seedDto.Name}' is invalid: {exception.Message}",
                exception);
        }

        ValidateSeedSpecificRules(seedDto);
    }

    private static void ValidateSeedSpecificRules(MovementSeedJsonDto seedDto)
    {
        if (seedDto.Id.HasValue &&
            seedDto.Origin != DataOrigin.BuiltIn)
        {
            throw new InvalidOperationException(
                $"Movement seed '{seedDto.Name}' has a fixed Id but is not BuiltIn. " +
                "Only built-in seed data should use stable seed ids for now.");
        }
    }

    private static void ValidateWorkTargetTypes(MovementSeedJsonDto seedDto)
    {
        if (seedDto.DefaultWorkTargetType == WorkTargetType.Unspecified)
        {
            throw new InvalidOperationException(
                "DefaultWorkTargetType must be provided.");
        }

        if (seedDto.AllowedWorkTargetTypes.Any(targetType => targetType == WorkTargetType.Unspecified))
        {
            throw new InvalidOperationException(
                "AllowedWorkTargetTypes contains an invalid Unspecified value.");
        }

        if (!seedDto.AllowedWorkTargetTypes.Contains(seedDto.DefaultWorkTargetType))
        {
            throw new InvalidOperationException(
                "AllowedWorkTargetTypes must contain DefaultWorkTargetType.");
        }
    }
}