using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;
using PRLab.Tools.Model;

namespace PRLab.Tools.Commands;

public sealed class ImportWorkoutCommandHandler(
    IEnumerable<IWorkoutSourceExtractor> sourceExtractors,
    IWorkoutImportInterpreter interpreter,
    IWorkoutImportContextFactory contextFactory,
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

        if (!File.Exists(filePath))
        {
            logger.LogWarning(
                $"Workout import file does not exist: {filePath}");

            return;
        }

        var content = await File.ReadAllBytesAsync(
            filePath,
            ct);

        var source = WorkoutImportSource.FromFile(
            Path.GetFileName(filePath),
            content,
            sourceReference: Path.GetFullPath(filePath));

        var extractor = sourceExtractors
            .FirstOrDefault(sourceExtractor =>
                sourceExtractor.CanHandle(source));

        if (extractor is null)
        {
            logger.LogWarning(
                $"No workout source extractor supports file '{source.FileName}'.");

            return;
        }

        logger.Log(
            nameof(ImportWorkoutCommandHandler),
            $"Using extractor: {extractor.GetType().Name}");

        var extractedSource = await extractor.ExtractAsync(
            source,
            ct);
        
        logger.Log(
            nameof(ImportWorkoutCommandHandler),
            $"Workout source extracted successfully.");

        var context = await contextFactory.CreateAsync(inputData.Profile, ct);
        
        var interpretation = await interpreter.InterpretAsync(extractedSource, context, ct);
        
        LogExtractedSource(extractedSource, false);
    }
    
    private void LogExtractedSource(
        ExtractedWorkoutSource extractedSource, bool verbose = true)
    {
        if(!verbose)
        {
            logger.Log(
                nameof(ImportWorkoutCommandHandler),
                extractedSource.NormalizedText);
            
            return;
        }

        foreach (var section in extractedSource.Sections)
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