using PRLab.Domain.Model.Value.Enum.Prescription;

namespace PRLab.Domain.Model.Value.Prescription;

public sealed record RestTarget
{
    public RestPolicy RestPolicy { get; init; } = RestPolicy.Fixed; // todo add to all other objects
    public int? Seconds { get; private set; }

    private RestTarget()
    {
        // EF Core
    }

    private RestTarget(int? seconds)
    {
        Seconds = ValidateSeconds(seconds);
    }

    public static RestTarget None()
    {
        return new RestTarget
        {
            Seconds = null,
        };
    }

    public static RestTarget SecondsDuration(int seconds)
    {
        return new RestTarget(seconds);
    }

    public bool IsEmpty()
    {
        return Seconds is null;
    }

    private static int? ValidateSeconds(int? seconds)
    {
        if (seconds is null)
        {
            return null;
        }

        if (seconds < 0)
        {
            throw new ArgumentException("Rest seconds cannot be negative.");
        }

        return seconds;
    }
}