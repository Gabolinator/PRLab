using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public interface IWorkoutImportInterpreter
{
    bool CanHandle(IExtractedWorkoutSource source);

    Task<IReadOnlyList<WorkoutImportDraft>> InterpretAsync(
        IExtractedWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default);
}

public interface IWorkoutImportInterpreter<in TExtractedSource>
    where TExtractedSource : IExtractedWorkoutSource
{
    Task<IReadOnlyList<WorkoutImportDraft>> InterpretAsync(
        TExtractedSource source,
        WorkoutImportContext context,
        CancellationToken ct = default);
}

public abstract class WorkoutImportInterpreter<TExtractedSource>
    : IWorkoutImportInterpreter,
        IWorkoutImportInterpreter<TExtractedSource>
    where TExtractedSource : IExtractedWorkoutSource
{
    public bool CanHandle(IExtractedWorkoutSource source)
    {
        return source is TExtractedSource;
    }

    public async Task<IReadOnlyList<WorkoutImportDraft>> InterpretAsync(
        IExtractedWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default)
    {
        if (source is not TExtractedSource typedSource)
        {
            throw new ArgumentException(
                $"Expected source of type '{typeof(TExtractedSource).Name}', " +
                $"but received '{source.GetType().Name}'.",
                nameof(source));
        }

        return await InterpretAsync(
            typedSource,
            context,
            ct);
    }

    public abstract Task<IReadOnlyList<WorkoutImportDraft>> InterpretAsync(
        TExtractedSource source,
        WorkoutImportContext context,
        CancellationToken ct = default);
}