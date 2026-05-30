namespace PRLab.Domain;

public static class DomainAssertions
{
    public static void ThrowIfNameInvalid<T>(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                $"{typeof(T).Name} name cannot be empty.",
                nameof(name));
        }
    }
}