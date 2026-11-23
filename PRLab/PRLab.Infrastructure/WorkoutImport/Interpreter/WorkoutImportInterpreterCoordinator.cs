using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

public sealed class WorkoutImportInterpreterCoordinator(
    IEnumerable<IWorkoutImportInterpreter> interpreters)
    : IWorkoutImportInterpreterCoordinator
{
    public async Task<WorkoutImportDraft> InterpretAsync(
        IExtractedWorkoutSource source,
        WorkoutImportContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        var matchingInterpreters = interpreters
            .Where(interpreter => interpreter.CanHandle(source))
            .ToList();

        if (matchingInterpreters.Count == 0)
        {
            throw new NotSupportedException(
                $"No workout import interpreter supports " +
                $"'{source.GetType().Name}'.");
        }

        if (matchingInterpreters.Count > 1)
        {
            var interpreterNames = string.Join(
                ", ",
                matchingInterpreters.Select(
                    interpreter => interpreter.GetType().Name));

            throw new InvalidOperationException(
                $"Multiple workout import interpreters support " +
                $"'{source.GetType().Name}': {interpreterNames}.");
        }

        return await matchingInterpreters[0].InterpretAsync(
            source,
            context,
            ct);
    }
}