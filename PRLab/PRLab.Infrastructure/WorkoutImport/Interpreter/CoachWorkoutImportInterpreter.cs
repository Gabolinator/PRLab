using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

public class CoachWorkoutImportInterpreter(IAppLogger logger)  : IWorkoutImportInterpreter
{
    public async Task<WorkoutImportDraft> InterpretAsync(
        ExtractedWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default)
    {
        //todo
        return new WorkoutImportDraft();
    }
}