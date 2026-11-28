using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Entity;

public sealed class Alias
{
    public const int MaxValueLength = 256;

    public AliasId Id { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public string ValueKey { get; private set; } = string.Empty;

    private Alias()
    {
    }

    private Alias(AliasId id, string value, string valueKey)
    {
        Id = id;
        Value = value;
        ValueKey = valueKey;
    }

    public static Alias New(string value)
    {
        var normalizedValue = FormatingUtilities.NormalizeName(value);
        var valueKey = GetValueKey(normalizedValue);

        return new Alias(AliasId.New(), normalizedValue, valueKey);
    }

    public static string GetValueKey(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Trim().Length > MaxValueLength)
        {
            throw new ArgumentException($"Alias must be no longer than {MaxValueLength} characters.", nameof(value));
        }

        var valueKey = FormatingUtilities.NormalizeNameKey(value);
        DomainGuard.NotEmptyName(valueKey, nameof(value));

        return valueKey;
    }
}
