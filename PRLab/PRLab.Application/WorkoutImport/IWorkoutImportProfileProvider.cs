using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.WorkoutImport;

public interface IWorkoutImportProfileProvider
{
    Task<WorkoutImportProfile> GetAsync(
        string? profileName = null,
        CancellationToken ct = default);
}