using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Application.Interface.UserService;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;


namespace PRLab.Infrastructure.WorkoutImport;

public sealed class WorkoutImportContextFactory(
    IExerciseRepository exerciseRepository,
    ICurrentUserService currentUserService,
    IWorkoutImportProfileProvider profileProvider,
    IAppLogger logger)
    : IWorkoutImportContextFactory
{
    public async Task<WorkoutImportContext> CreateAsync(
        string? profileName = null,
        CancellationToken ct = default)
    {
        var profile = await profileProvider.GetAsync(
            profileName,
            ct);
        
        var exercises = await exerciseRepository.ListAsync(ct);
        logger.Log(nameof(WorkoutImportContextFactory), $"Retrieved exercises: {exercises.Count}");

        var userId = currentUserService.GetRequiredUserId();
        var exerciseReferences = exercises.Select(exercise => new WorkoutImportExerciseReference
        {
            Id = exercise.Id,
            Name = exercise.Name,
            NameKey = exercise.NameKey
        }).ToList();

        return new WorkoutImportContext
        {
            UserId = userId,
            Profile = profile,
            Exercises = exerciseReferences
        };
    }
}
