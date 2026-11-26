using PRLab.Application.WorkoutImport.Model;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter.Extensions;

internal static class CoachExcelWorkoutImportSourceProfileExtensions
{
    public static ILookup<int, WorkoutImportProperty> GetCoachColumnMapping(
        this ExcelWorkoutImportSourceProfile profile)
    {
        var exerciseColumn = profile.GetRequiredCell(WorkoutImportProperty.Exercise);
        var orderColumn = GetOptionalColumn(profile, WorkoutImportProperty.Order);
        var detailsColumn = GetOptionalColumn(profile, WorkoutImportProperty.Details);
        var notesColumn = GetOptionalColumn(profile, WorkoutImportProperty.Notes);
        (int Column, WorkoutImportProperty Property)[] columns =
        [
            (0, WorkoutImportProperty.Date),
            (exerciseColumn, WorkoutImportProperty.Exercise),
            (orderColumn, WorkoutImportProperty.Order),
            (detailsColumn, WorkoutImportProperty.Details),
            (notesColumn, WorkoutImportProperty.Notes)
        ];
        var configuredColumns = columns.Where(column => column.Column >= 0);
        return configuredColumns.ToLookup(column => column.Column, column => column.Property);
    }

    private static int GetOptionalColumn(ExcelWorkoutImportSourceProfile profile, WorkoutImportProperty property) =>
        profile.TryGetColumn(property, out var index) ? index : -1;
}
