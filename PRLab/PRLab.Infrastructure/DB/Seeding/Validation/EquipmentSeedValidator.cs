using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Policies;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

namespace PRLab.Infrastructure.DB.Seeding.Validation;

public static class EquipmentSeedValidator
{
    public static void Validate(EquipmentSeedJsonDto seedDto)
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
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Equipment seed '{seedDto.Name}' is invalid: {exception.Message}",
                exception);
        }

        ValidateSeedSpecificRules(seedDto);
    }

    private static void ValidateSeedSpecificRules(EquipmentSeedJsonDto seedDto)
    {
        if (seedDto.Id.HasValue &&
            seedDto.Origin != DataOrigin.BuiltIn)
        {
            throw new InvalidOperationException(
                $"Equipment seed '{seedDto.Name}' has a fixed Id but is not BuiltIn. " +
                "Only built-in seed data should use stable seed ids for now.");
        }
    }
}