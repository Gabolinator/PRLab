using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.Validation;

internal static class AliasSeedValidator
{
    public static void Validate(string entityName, IReadOnlyList<string>? aliases)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in aliases ?? [])
        {
            string key;
            try
            {
                key = Alias.GetValueKey(value);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"Seed '{entityName}' has an invalid alias: {exception.Message}", exception);
            }

            if (!keys.Add(key))
            {
                throw new InvalidOperationException($"Seed '{entityName}' has duplicate alias '{value}'.");
            }
        }
    }
}
