using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

public sealed class CoachExcelWorkoutImportInterpreter(
    IAppLogger logger)
    : WorkoutImportInterpreter<ExtractedExcelWorkoutSource>
{
    public override Task<WorkoutImportDraft> InterpretAsync(
        ExtractedExcelWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default)
    {
        logger.LogWarning(nameof(CoachExcelWorkoutImportInterpreter),"Method not yet implemented");

        var interpretation = new WorkoutImportDraft();
        return Task.FromResult(interpretation);
    }
}