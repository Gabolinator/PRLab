using PRLab.Domain;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Policies;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

namespace PRLab.Infrastructure.DB.Seeding.Validation;

public static class MovementCategorySeedValidator
{
    public static void Validate(MovementCategorySeedJsonDto seedDto)
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
                $"Movement Categoty seed '{seedDto.Name}' is invalid: {exception.Message}",
                exception);
        }
    }
}