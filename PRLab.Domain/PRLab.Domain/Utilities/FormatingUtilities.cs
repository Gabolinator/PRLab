namespace PRLab.Utilities;

public static class FormatingUtilities
{
    public static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
            
        return name.Trim();
    }
}