namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutImportDraft
{
    public string? Name { get; init; }

    public string? Description { get; init; }
    
    public DateTime Date { get; init; }

    public IReadOnlyList<WorkoutBlockImportDraft> Blocks { get; init; } = [];

    public IReadOnlyList<WorkoutImportWarning> Warnings { get; init; } = [];

    public string? SourceReference { get; init; }
}