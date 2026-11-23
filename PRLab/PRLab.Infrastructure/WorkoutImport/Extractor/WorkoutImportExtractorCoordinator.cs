using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.WorkoutImport.Extractor;

public class WorkoutImportExtractorCoordinator(
    IEnumerable<IWorkoutSourceExtractor> sourceExtractors,
    IAppLogger logger) : IWorkoutImportExtractorCoordinator
{
    public async Task<IExtractedWorkoutSource> ExtractAsync(
        string? filePath,
        CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
        {
            logger.LogWarning(
                nameof(WorkoutImportExtractorCoordinator),
                $"Workout import file does not exist: {filePath}");

            throw new FileNotFoundException($"Workout import file does not exist: {filePath}");
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
            throw new NotSupportedException(
                $"No workout source extractor supports file '{source.FileName}'.");
        }

        logger.Log(
            nameof(WorkoutImportExtractorCoordinator),
            $"Using extractor: {extractor.GetType().Name}");
        
        return await extractor.ExtractAsync(source, ct);
    }
}