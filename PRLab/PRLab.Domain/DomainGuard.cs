namespace PRLab.Domain;

public static class DomainGuard
{
    public static void NotEmptyName(
        string? name,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Name cannot be empty.",
                parameterName);
        }
    }

    public static void ValidOptionalId(
        Guid? id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Id cannot be empty. Omit the Id property or provide a valid id.",
                parameterName);
        }
    }

    public static void ValidRequiredId(
        Guid? id,
        string parameterName)
    {
        if (!id.HasValue)
        {
            throw new ArgumentNullException(
                parameterName,
                "Id cannot be null. Provide a valid id.");
        }

        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Id cannot be empty. Provide a valid id.",
                parameterName);
        }
    }

    public static void NotEmptyId(
        Guid id,
        string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Id cannot be empty.",
                parameterName);
        }
    }
}