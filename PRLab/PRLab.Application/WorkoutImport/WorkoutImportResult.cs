using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public sealed record WorkoutImportResult
{
    public required IExtractedWorkoutSource ExtractedSource { get; init; }

    public required IReadOnlyList<WorkoutImportDraft> Drafts { get; init; }
}