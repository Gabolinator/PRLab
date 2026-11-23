using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

public sealed class ExcelWorkoutImportColumnMapping
{
    public int CellIndex { get; init; }

    public WorkoutImportProperty Property { get; init; }
}