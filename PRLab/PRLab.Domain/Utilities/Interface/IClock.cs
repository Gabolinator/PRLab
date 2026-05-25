namespace PRLab.Domain.Utilities.Interface;

public interface IClock
{
    public DateTimeOffset UtcNow { get; }
}