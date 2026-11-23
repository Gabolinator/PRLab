using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

public sealed class ExcelWorkoutImportSourceProfile
    : WorkoutImportSourceProfile
{
    public override WorkoutImportSourceFormat Format =>
        WorkoutImportSourceFormat.Excel;

    public IReadOnlyList<ExcelWorkoutImportColumnMapping> Columns { get; init; } = [];

    public bool TryGetColumn(
        WorkoutImportProperty property,
        out int columnIndex)
    {
        var mapping = Columns
            .SingleOrDefault(column =>
                column.Property == property);

        if (mapping is null)
        {
            columnIndex = -1;
            return false;
        }

        columnIndex = mapping.CellIndex;
        return true;
    }

    public int GetRequiredCell(
        WorkoutImportProperty property)
    {
        if (TryGetColumn(
                property,
                out var cellIndex))
        {
            return cellIndex;
        }

        throw new InvalidOperationException(
            $"No Excel column mapping exists for workout import property " +
            $"'{property}'.");
    }
}