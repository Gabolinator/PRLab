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
        logger.LogWarning(nameof(CoachExcelWorkoutImportInterpreter),"Method not yet implemented");

        
        // it doesnt mean one sheet is one draft 
        // there could be many workouts on one sheet
        // how to know if one is already imported ? we still draft it and wait for translation to model ? 
        
        // use date as separator
        //start by finding different dates
        IReadOnlyList<DateTime> allDistinctDates = GetDistinctDates(context, source.Sheets);
        logger.Log(nameof(CoachExcelWorkoutImportInterpreter), $"Found {allDistinctDates.Count} date distinct : {string.Join(", ",  allDistinctDates)}");
          
        // we need that amount of drafts 
        // we pick all rows with those days 
        // and start try to figure out what is what
        
        
        
        var interpretation = new WorkoutImportDraft();
        var drafts = new List<WorkoutImportDraft>(){interpretation};

        drafts = CreateDraftsFromDates(allDistinctDates, source.Sheets);
        
        return Task.FromResult<IReadOnlyList<WorkoutImportDraft>>(drafts);
    }

    private List<WorkoutImportDraft> CreateDraftsFromDates(IReadOnlyList<DateTime> dates, IReadOnlyList<ExtractedWorkoutSheet> sourceSheets)
    {
        var allRow = sourceSheets.SelectMany(sheet => sheet.Rows).ToList();
        foreach (var date in dates)
        {
            
        }
    }

    private IReadOnlyList<DateTime> GetDistinctDates(WorkoutImportContext context, IReadOnlyList<ExtractedWorkoutSheet> sheets)
    {
        var dateColumn = context.Profile.DateColumn;
        var hasDateColumn = dateColumn >= 0;

        var cells = hasDateColumn
            ? sheets.SelectMany(sheet => sheet.Rows.Select(row => row.Cells[dateColumn!.Value]))
            : sheets.SelectMany(sheet => sheet.Rows.SelectMany(row => row.Cells));
        
        return GetDistinctDatesFromCells(cells);
    }

    private IReadOnlyList<DateTime> GetDistinctDatesFromCells(IEnumerable<ExtractedWorkoutCell> cells)
    {
        if (!cells.Any())
        {
            return [];
        }

        var dates = new List<DateTime>();
        
        foreach (var cell in cells)
        {
            if (!DateTime.TryParse(cell.Value, out var date))
            {
                continue;
            }
            
            dates.Add(date);
        }

        return dates.Distinct().ToList();
    }
}