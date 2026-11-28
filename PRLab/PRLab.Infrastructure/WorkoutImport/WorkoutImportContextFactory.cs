using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Application.Interface.UserService;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;

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
        var exerciseReferences =
            exercises
                .Select(CreateExerciseReference)
                .ToList();

        return new WorkoutImportContext
        {
            UserId = userId,
            Profile = profile,
            Exercises = exerciseReferences
        };
    }

    private static WorkoutImportExerciseReference CreateExerciseReference(Exercise exercise)
    {
        var movements =
            exercise.Steps
                .Select(step => step.Movement)
                .DistinctBy(movement => movement.Id)
                .ToList();

        var defaults =
            movements
                .Select(movement => movement.DefaultWorkTargetType)
                .Distinct()
                .ToList();
        var allowedTypes =
            movements
                .Select(movement => movement.AllowedWorkTargets
                    .Select(target => target.TargetType)
                    .Append(movement.DefaultWorkTargetType))
                .ToList();
        var sharedTypes = allowedTypes.Count == 0
            ? null
            : allowedTypes
                .Aggregate((shared, next) => shared.Intersect(next))
                .Distinct()
                .ToArray();

        return new WorkoutImportExerciseReference
        {
            Id = exercise.Id,
            Name = exercise.Name,
            NameKey = exercise.NameKey,
            DefaultWorkTargetType = defaults.Count == 1 ? defaults[0] : null,
            AllowedWorkTargetTypes = sharedTypes,
            SupportsExternalLoad = exercise.Steps.Count > 0 && exercise.Steps.All(SupportsExternalLoad)
        };
    }

    private static bool SupportsExternalLoad(ExerciseSteps step) =>
        step.LoadTarget.Type is LoadTargetType.ExternalLoad or LoadTargetType.AddedBodyWeightLoad ||
        step.Movement.EquipmentRequirements.Any(requirement => WorkoutImportEquipment.SupportsExternalLoad(requirement.Equipment.NameKey));
}
