using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Identifier;
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
            context.Exercises,
            context.Profile,
            excelProfile);
        
        return Task.FromResult<IReadOnlyList<WorkoutImportDraft>>(drafts);
    }

    private IReadOnlyList<WorkoutImportDraft> CreateDraftsFromDates(
        IReadOnlyList<DateTime> dates,
        IReadOnlyList<ExtractedWorkoutSheet> sourceSheets,
        IReadOnlyList<WorkoutImportExerciseReference> exerciseReferences,
        WorkoutImportProfile profile,
        ExcelWorkoutImportSourceProfile excelProfile)
    {
        var allRow = sourceSheets.SelectMany(sheet => sheet.Rows).ToList();
        var dateCell = excelProfile.GetRequiredCell(WorkoutImportProperty.Date);
        
        var drafts = new List<WorkoutImportDraft>();
        
        //todo
        foreach (var date in dates)
        {
            //get all row with date
            //this is the content of the training
            var matchingRows = allRow.Where(row => WorkoutInterpreterHelper.IsSameDate(row.Cells[dateCell].Value, date));
           
            //each row is either an exercise of a block (if many exercise)
            // how do we know ? 
            //try to get exercices 
            var items = InterpretRows(
                matchingRows,
                exerciseReferences,
                profile,
                excelProfile);
            
            var draft = new WorkoutImportDraft
            {
                Date = date,
            };
            
            drafts.Add(draft);
        }
        
        return drafts;
    }

    private IReadOnlyList<DraftItem> InterpretRows(
        IEnumerable<ExtractedWorkoutRow> rows,
        IReadOnlyList<WorkoutImportExerciseReference> exerciseReferences,
        WorkoutImportProfile profile,
        ExcelWorkoutImportSourceProfile excelProfile)
    {
        // get the cell at cellIndex , than try to have a match to an exercices
        // preserve order
        return rows
            .Select(row => InterpretRow(
                row,
                exerciseReferences,
                profile,
                excelProfile))
            .ToList();
    }
    
    private DraftItem InterpretRow(
        ExtractedWorkoutRow row,
        IReadOnlyList<WorkoutImportExerciseReference> exerciseReferences,
        WorkoutImportProfile profile,
        ExcelWorkoutImportSourceProfile excelProfile
        )
    {
        // get the cell at cellIndex , than try to have a match to an exercices
        // preserve order
        var dateCell = excelProfile.GetRequiredCell(
            WorkoutImportProperty.Date);

        var exercisesCell = excelProfile.GetRequiredCell(
            WorkoutImportProperty.Exercise);
        var orderCell = excelProfile.GetRequiredCell(
            WorkoutImportProperty.Order);
        
        var exerciseCellValue = row.Cells[exercisesCell].Value;

        // we get all parts of the string
        // then analyse each part to extract what type of dratfitem it is
        // get all present exercises names etc
        
        var allParts = WorkoutInterpreterHelper.GetAllParts(exerciseCellValue, profile.Separators);
        var exerciseNames = WorkoutInterpreterHelper.GetExercisesNames(profile.ExerciseAliases, allParts);
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
        IEnumerable<ExtractedWorkoutCell> cells) =>
        WorkoutInterpreterHelper
            .GetDistinctDatesFromStrings(cells
            .Select(cell => cell.Value));
    
}

public abstract class DraftItem
{
    public required string SourceText { get; init; }
    
    public float Confidence { get; init; }
}

public class ExerciseMatch : DraftItem
{
    public string? ExerciseName { get; init; }
    
    public ExerciseId? ExerciseIdMatch { get; init; }
}

//make other draft item

// is the row a block a sequence etc ? 
public class RowDraft
{
    public required string SourceText { get; init; }
    
    public DraftItem? DraftItem { get; init; }
}