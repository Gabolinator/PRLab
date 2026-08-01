using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Workout;
using PRLab.Application.Interface.UserService;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;
using PRLab.Infrastructure.DB.Seeding.Validation;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonWorkoutSeedFactory(
    ISystemUserProvider userService,
    ISeedingConfig config,
    IWorkoutSeedRelationResolver relationResolver)
    : BaseJsonSeedFactory<Workout, WorkoutSeedJsonDto>(userService, config), IWorkoutSeedFactory
{
    protected override EntityType Entity => EntityType.Workout;

    public override SeedItem<Workout> ToSeedItem(SeedExecutionOptions options, WorkoutSeedJsonDto seedDto)
    {
        throw new NotSupportedException(
            "Workout seeds require an exercise catalog. Use CreateInitialData(...catalog).");
    }

    public override void Validate(WorkoutSeedJsonDto seedDto)
        => WorkoutSeedValidator.Validate(seedDto);

    public IReadOnlyList<SeedItem<Workout>> CreateInitialData(
        SeedExecutionOptions options,
        ExerciseSeedCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var seedDtos = LoadSeedDtos();

        return seedDtos
            .Select(seedDto => ToSeedItem(options, seedDto, catalog))
            .ToList();
    }

    private SeedItem<Workout> ToSeedItem(
        SeedExecutionOptions options,
        WorkoutSeedJsonDto seedDto,
        ExerciseSeedCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalog);

        Validate(seedDto);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var workout = CreateWorkout(
            options,
            seedDto,
            description);

        ApplyEstimatedDuration(
            workout,
            seedDto);

        relationResolver.ApplyRelations(
            workout,
            seedDto,
            catalog,
            options,
            SeedUser);

        return new SeedItem<Workout>(
            SeedKeyGenerator.GenerateWorkoutKey(workout),
            workout,
            options.ResolveAction(seedDto.Action));
    }

    private Workout CreateWorkout(
        SeedExecutionOptions options,
        WorkoutSeedJsonDto seedDto,
        Description description)
    {
        var shouldUseSeedId =
            seedDto.Id.HasValue &&
            !options.IgnoreTopLevelIds;

        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn when shouldUseSeedId => Workout.NewBuiltInWithId(
                id: WorkoutId.FromGuid(seedDto.Id!.Value),
                name: seedDto.Name,
                description: description,
                createdBy: SeedUser,
                visibilityScope: seedDto.VisibilityScope),

            DataOrigin.BuiltIn => Workout.NewBuiltIn(
                name: seedDto.Name,
                description: description,
                createdBy: SeedUser,
                visibilityScope: seedDto.VisibilityScope),

            _ => throw new NotSupportedException(
                $"JSON workout seeding currently only supports {nameof(DataOrigin.BuiltIn)} workouts. " +
                $"Workout seed '{seedDto.Name}' has origin '{seedDto.Origin}'.")
        };
    }

    private static void ApplyEstimatedDuration(
        Workout workout,
        WorkoutSeedJsonDto seedDto)
    {
        if (seedDto.EstimatedDuration is null)
        {
            return;
        }

        workout.ChangeEstimatedDuration(
            seedDto.EstimatedDuration.ToEstimatedDuration());
    }
}