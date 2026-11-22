using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Application.Interface.UserService;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;


namespace PRLab.Infrastructure.WorkoutImport;

public sealed class WorkoutImportContextFactory(
    IExerciseRepository exerciseRepository,
    ICurrentUserService currentUserService,
    IWorkoutImportProfileProvider profileProvider)
    : IWorkoutImportContextFactory
{
    public async Task<WorkoutImportContext> CreateAsync(
        string? profileName = null,
        CancellationToken ct = default)
    {
        var profile = await profileProvider.GetAsync(
            profileName,
            ct);

        // todo
        //var exercises = await exerciseRepository.ListAsync(ct);

        return new WorkoutImportContext
        {
            UserId = currentUserService.GetRequiredUserId(),

            Profile = profile,

            Exercises = [],
            //todo
            // Exercises = exercises
            //     .Select(exercise =>
            //         new WorkoutImportExerciseReference
            //         {
            //             Id = exercise.Id,
            //             Name = exercise.Name,
            //             NameKey = exercise.NameKey
            //         })
            //     .ToList()
        };
    }
}