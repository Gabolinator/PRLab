using PRLab.Domain.Value.Enum.Prescription;

namespace PRLab.Domain.Value;

public sealed record RepExecutionDetails
{
    public int? EccentricSeconds { get; private set; }

    public int? BottomPauseSeconds { get; private set; }

    public int? ConcentricSeconds { get; private set; }

    public int? TopPauseSeconds { get; private set; }

    public RepPhaseExecutionIntent? EccentricIntent { get; private set; }

    public RepPhaseExecutionIntent? BottomIntent { get; private set; }

    public RepPhaseExecutionIntent? ConcentricIntent { get; private set; }

    public RepPhaseExecutionIntent? TopIntent { get; private set; }

    public string? Intent { get; private set; }

    private RepExecutionDetails()
    {
        // EF Core
    }

    private RepExecutionDetails(
        int? eccentricSeconds,
        int? bottomPauseSeconds,
        int? concentricSeconds,
        int? topPauseSeconds,
        RepPhaseExecutionIntent? eccentricIntent,
        RepPhaseExecutionIntent? bottomIntent,
        RepPhaseExecutionIntent? concentricIntent,
        RepPhaseExecutionIntent? topIntent,
        string? intent)
    {
        EccentricSeconds = ValidateOptionalSeconds(eccentricSeconds);
        BottomPauseSeconds = ValidateOptionalSeconds(bottomPauseSeconds);
        ConcentricSeconds = ValidateOptionalSeconds(concentricSeconds);
        TopPauseSeconds = ValidateOptionalSeconds(topPauseSeconds);
        EccentricIntent = eccentricIntent;
        BottomIntent = bottomIntent;
        ConcentricIntent = concentricIntent;
        TopIntent = topIntent;
        Intent = NormalizeIntent(intent);
    }

    public static RepExecutionDetails Empty()
    {
        return new RepExecutionDetails(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );
    }

    public static RepExecutionDetails New(
        int? eccentricSeconds = null,
        int? bottomPauseSeconds = null,
        int? concentricSeconds = null,
        int? topPauseSeconds = null,
        RepPhaseExecutionIntent? eccentricIntent = null,
        RepPhaseExecutionIntent? bottomIntent = null,
        RepPhaseExecutionIntent? concentricIntent = null,
        RepPhaseExecutionIntent? topIntent = null,
        string? intent = null)
    {
        return new RepExecutionDetails(
            eccentricSeconds,
            bottomPauseSeconds,
            concentricSeconds,
            topPauseSeconds,
            eccentricIntent,
            bottomIntent,
            concentricIntent,
            topIntent,
            intent
        );
    }

    public bool IsEmpty()
    {
        return EccentricSeconds is null
               && BottomPauseSeconds is null
               && ConcentricSeconds is null
               && TopPauseSeconds is null
               && EccentricIntent is null
               && BottomIntent is null
               && ConcentricIntent is null
               && TopIntent is null
               && string.IsNullOrWhiteSpace(Intent);
    }

    private static int? ValidateOptionalSeconds(int? seconds)
    {
        if (seconds is null)
        {
            return null;
        }

        if (seconds < 0)
        {
            throw new ArgumentException("Execution detail seconds cannot be negative.");
        }

        return seconds;
    }

    private static string? NormalizeIntent(string? intent)
    {
        if (string.IsNullOrWhiteSpace(intent))
        {
            return null;
        }

        return intent.Trim();
    }
}