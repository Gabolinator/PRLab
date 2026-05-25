namespace PRLab.Domain.Utilities;

public class GuidGenerator
{
    public Guid New() => Guid.NewGuid();
    public Guid AdminId => Guid.Empty;
}