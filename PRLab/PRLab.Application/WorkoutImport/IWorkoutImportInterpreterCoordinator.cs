using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public interface IWorkoutImportInterpreterCoordinator
{
    Task<WorkoutImportDraft> InterpretAsync(
        IExtractedWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default);
}