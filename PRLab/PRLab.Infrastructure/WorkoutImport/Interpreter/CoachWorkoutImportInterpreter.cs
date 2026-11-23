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
    public override Task<IReadOnlyList<WorkoutImportDraft>> InterpretAsync(
        ExtractedExcelWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default)
    {
        var excelProfile =
            context.Profile.GetRequiredSourceProfile<
                ExcelWorkoutImportSourceProfile>();

        var dateCell = excelProfile.GetRequiredCell(
            WorkoutImportProperty.Date);

        // it doesnt mean one sheet is one draft 
        // there could be many workouts on one sheet
        // how to know if one is already imported ? we still draft it and wait for translation to model ? 
        
        // use date as separator
        //start by finding different dates
        var dates = GetDistinctDates(
            source.Sheets,
            dateCell);
        
        logger.Log(
            nameof(CoachExcelWorkoutImportInterpreter),
            $"Found {dates.Count} distinct dates: " +
            $"{string.Join(", ", dates)}");
          
        // we need that amount of drafts 
        // we pick all rows with those days 
        // and start try to figure out what is what
        var drafts = CreateDraftsFromDates(
            dates,
            source.Sheets,
            excelProfile);

        
        return Task.FromResult<IReadOnlyList<WorkoutImportDraft>>(drafts);
    }

    private IReadOnlyList<WorkoutImportDraft> CreateDraftsFromDates(IReadOnlyList<DateTime> dates, IReadOnlyList<ExtractedWorkoutSheet> sourceSheets, ExcelWorkoutImportSourceProfile excelProfile)
    {
        var allRow = sourceSheets.SelectMany(sheet => sheet.Rows).ToList();
        var drafts = new List<WorkoutImportDraft>();
        var dateCell = excelProfile.GetRequiredCell(
            WorkoutImportProperty.Date);
        
        //todo
        foreach (var date in dates)
        {
            //get all row with date
            //this is the content of the training
            var matchingRows = allRow.Where(row => IsDate(row.Cells[dateCell], date));
           
            //each row is either an exercise of a block (if many exercise)
            // how do we know ? 
            //try to get exercices 
            var exercices = GetExercises(
                matchingRows,
                excelProfile.GetRequiredCell(
                WorkoutImportProperty.Exercise), 
                excelProfile.GetRequiredCell(
                WorkoutImportProperty.Order));
            
            var draft = new WorkoutImportDraft
            {
                Date = date,
            };
            
            drafts.Add(draft);
        }
        
        return drafts;
    }

    private IReadOnlyList<> GetExercises(IEnumerable<ExtractedWorkoutRow> rows, int exercisesCell, int orderCell)
    {
        // get the cell at cellIndex , than try to have a match to an exercices
        // preserve order
        
    }

    private bool IsDate(ExtractedWorkoutCell cell, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(cell.Value))
        {
            return false;
        }
        
        return DateTime.TryParse(cell.Value, out var cellDate) && cellDate.Date == date;
    }

    private IReadOnlyList<DateTime> GetDistinctDates(
        IReadOnlyList<ExtractedWorkoutSheet> sheets,
        int dateCellIndex)
    {
        var cells = sheets
            .SelectMany(sheet => sheet.Rows)
            .Where(row =>
                row.Cells.Count > dateCellIndex)
            .Select(row =>
                row.Cells[dateCellIndex]);

        return GetDistinctDatesFromCells(cells);
    }

    private static IReadOnlyList<DateTime> GetDistinctDatesFromCells(
        IEnumerable<ExtractedWorkoutCell> cells)
    {
        return cells
            .Select(cell =>
                DateTime.TryParse(
                    cell.Value,
                    out var date)
                    ? date.Date
                    : (DateTime?)null)
            .Where(date => date.HasValue)
            .Select(date => date!.Value)
            .Distinct()
            .OrderBy(date => date)
            .ToList();
    }
}