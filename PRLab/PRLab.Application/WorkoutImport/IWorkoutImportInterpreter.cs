using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public interface IWorkoutImportInterpreter
{
    Task<WorkoutImportDraft> InterpretAsync(
        ExtractedWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default);
}