namespace PRLab.Application.Models.ImportDraft;

public enum WorkoutImportIssueSeverity
{
    Information,
    Warning,
    Error
}

public class WorkoutImportWarning
{
    public required WorkoutImportIssueSeverity Severity { get; init; }
    
    public required string Message { get; init; }
}