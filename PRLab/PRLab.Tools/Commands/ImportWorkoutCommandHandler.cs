using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;
using PRLab.Tools.Model;

namespace PRLab.Tools.Commands;

public sealed class ImportWorkoutCommandHandler(
    IWorkoutImportService importService,
    IAppLogger logger)
{
    public async Task HandleAsync(
        PRToolCommandInputData inputData,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(inputData);

        if (!inputData.IsValidForWorkoutImport())
        {
            logger.LogWarning(
                "Workout import requires a valid file path.");

            return;
        }

        var filePath = inputData.FilePath!;

        var draft = await importService.ExtractAndInterpretAsync(filePath, inputData.Profile, ct);
        LogExtractedSource(draft.ExtractedSource, false);
    }

    private void LogExtractedSource(IExtractedWorkoutSource extractedSource, bool verbose)
    {
        switch (extractedSource)
        {
            case ExtractedExcelWorkoutSource excelSource:
                LogExtractedExcelSource(
                    excelSource,
                    verbose);
                break;
            
            default:
                logger.LogWarning(
                    $"No logger implemented for extracted source type " +
                    $"'{extractedSource.GetType().Name}'.");
                break;
        }
    }

    private void LogExtractedExcelSource(
        ExtractedExcelWorkoutSource extractedExcelSource, bool verbose = true)
    {
        if(!verbose)
        {
            logger.Log(
                nameof(ImportWorkoutCommandHandler),
                extractedExcelSource.NormalizedText);
            
            return;
        }

        foreach (var section in extractedExcelSource.Sheets)
        {
            logger.Log(
                nameof(ImportWorkoutCommandHandler),
                $"Section [{section.Sequence}] '{section.Name}' " +
                $"Rows: {section.Rows.Count}");

            foreach (var row in section.Rows)
            {
                logger.Log(
                    nameof(ImportWorkoutCommandHandler),
                    $"  Row {row.RowNumber}: {row.RawText}");

                foreach (var cell in row.Cells)
                {
                    logger.Log(
                        nameof(ImportWorkoutCommandHandler),
                        $"    [{cell.ColumnIndex}] " +
                        $"{cell.ColumnName} = '{cell.Value}' " +
                        $"[{cell.SourceReference}]");
                }
            }
        }
    }
}