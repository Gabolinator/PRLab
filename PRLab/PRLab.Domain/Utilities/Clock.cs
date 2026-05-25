using PRLab.Domain.Utilities.Interface;

namespace PRLab.Domain.Utilities;

public class Clock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}