namespace PRLab.Domain.Value;

public sealed record RepExecutionDetails
{
    public int? EccentricSeconds { get; private set; }

    public int? BottomPauseSeconds { get; private set; }

    public int? ConcentricSeconds { get; private set; }

    public int? TopPauseSeconds { get; private set; }

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
        string? intent)
    {
        EccentricSeconds = ValidateOptionalSeconds(eccentricSeconds);
        BottomPauseSeconds = ValidateOptionalSeconds(bottomPauseSeconds);
        ConcentricSeconds = ValidateOptionalSeconds(concentricSeconds);
        TopPauseSeconds = ValidateOptionalSeconds(topPauseSeconds);
        Intent = NormalizeIntent(intent);
    }

    public static RepExecutionDetails Empty()
    {
        return new RepExecutionDetails(
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
        string? intent = null)
    {
        return new RepExecutionDetails(
            eccentricSeconds,
            bottomPauseSeconds,
            concentricSeconds,
            topPauseSeconds,
            intent
        );
    }

    public bool IsEmpty()
    {
        return EccentricSeconds is null
            && BottomPauseSeconds is null
            && ConcentricSeconds is null
            && TopPauseSeconds is null
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