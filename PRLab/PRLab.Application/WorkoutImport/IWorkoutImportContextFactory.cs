using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public interface IWorkoutImportContextFactory
{
    Task<WorkoutImportContext> CreateAsync(
        string? profileName = null,
        CancellationToken ct = default);
}