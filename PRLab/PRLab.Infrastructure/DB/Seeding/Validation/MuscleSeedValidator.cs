using PRLab.Domain;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;

namespace PRLab.Infrastructure.DB.Seeding.Validation;

public static class MuscleSeedValidator
{
    public static void Validate(MuscleSeedJsonDto seedDto)
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
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Muscle seed '{seedDto.Name}' is invalid: {exception.Message}",
                exception);
        }
    }
}