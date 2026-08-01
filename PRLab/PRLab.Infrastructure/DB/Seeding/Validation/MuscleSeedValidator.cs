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
            
            ValidateFunctions(seedDto.Functions);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Muscle seed '{seedDto.Name}' is invalid: {exception.Message}",
                exception);
        }
    }
    
    private static void ValidateFunctions(
        IReadOnlyCollection<MuscleFunctionSeedJsonDto> functions)
    {
        ArgumentNullException.ThrowIfNull(functions);

        foreach (var function in functions)
        {
            if (!Enum.IsDefined(function.Function))
            {
                throw new InvalidOperationException(
                    $"Unsupported muscle function '{function.Function}'.");
            }

            if (!Enum.IsDefined(function.Role))
            {
                throw new InvalidOperationException(
                    $"Unsupported muscle function role '{function.Role}'.");
            }
        }

        var duplicateFunctions = functions
            .GroupBy(function => function.Function)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateFunctions.Count > 0)
        {
            throw new InvalidOperationException(
                $"Muscle contains duplicate functions: " +
                $"{string.Join(", ", duplicateFunctions)}.");
        }
    }
}