namespace PRLab.Application.WorkoutImport.Config;

public sealed class WorkoutImportOptions
{
    public const string SectionName = "WorkoutImport";

    public string ProfileDirectory { get; init; }
        = "WorkoutImport/Profiles";

    public string? DefaultProfile { get; init; }
}