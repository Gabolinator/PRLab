namespace PRLab.Domain.Value;

public sealed record RepExecutionDetails
{
    public int? EccentricSeconds { get; private set; }

    public int? BottomPauseSeconds { get; private set; }

    public int? ConcentricSeconds { get; private set; }

    public int? TopPauseSeconds { get; private set; }

    public DomainEnum.RepPhaseExecutionIntent? EccentricIntent { get; private set; }

    public DomainEnum.RepPhaseExecutionIntent? BottomIntent { get; private set; }

    public DomainEnum.RepPhaseExecutionIntent? ConcentricIntent { get; private set; }

    public DomainEnum.RepPhaseExecutionIntent? TopIntent { get; private set; }

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
        DomainEnum.RepPhaseExecutionIntent? eccentricIntent,
        DomainEnum.RepPhaseExecutionIntent? bottomIntent,
        DomainEnum.RepPhaseExecutionIntent? concentricIntent,
        DomainEnum.RepPhaseExecutionIntent? topIntent,
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
        DomainEnum.RepPhaseExecutionIntent? eccentricIntent = null,
        DomainEnum.RepPhaseExecutionIntent? bottomIntent = null,
        DomainEnum.RepPhaseExecutionIntent? concentricIntent = null,
        DomainEnum.RepPhaseExecutionIntent? topIntent = null,
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