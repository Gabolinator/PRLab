using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Workout;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonWorkoutSeedFactory(
    IUserService userService,
    ISeedingConfig config,
    IWorkoutSeedRelationResolver relationResolver)
    : BaseJsonSeedFactory<Workout, WorkoutSeedJsonDto>(userService, config), IWorkoutSeedFactory
{
    protected override EntityType Entity => EntityType.Workout;

    public override SeedItem<Workout> ToSeedItem(WorkoutSeedJsonDto seedDto)
    {
        throw new NotSupportedException(
            "Workout seeds require an exercise catalog. Use CreateInitialData(...catalog).");
    }

    public IReadOnlyList<SeedItem<Workout>> CreateInitialData(
        ExerciseSeedCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var seedDtos = LoadSeedDtos();

        return seedDtos
            .Select(seedDto => ToSeedItem(seedDto, catalog))
            .ToList();
    }

    private SeedItem<Workout> ToSeedItem(
        WorkoutSeedJsonDto seedDto,
        ExerciseSeedCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalog);

        ValidateSeedDto(seedDto);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var workout = CreateWorkout(
            seedDto,
            description);

        ApplyEstimatedDuration(
            workout,
            seedDto);

        relationResolver.ApplyRelations(
            workout,
            seedDto,
            catalog,
            SeedUser);

        return new SeedItem<Workout>(
            SeedKeyGenerator.GenerateWorkoutKey(workout),
            workout,
            seedDto.Action);
    }

    private static void ValidateSeedDto(
        WorkoutSeedJsonDto seedDto)
    {
        if (string.IsNullOrWhiteSpace(seedDto.Name))
        {
            throw new InvalidOperationException("Workout seed name cannot be empty.");
        }

        if (seedDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' has an empty id. Omit the Id property or provide a valid id.");
        }

        if (seedDto.Origin == DataOrigin.BuiltIn && seedDto.OwnerUserId is not null)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' is BuiltIn and should not provide OwnerUserId.");
        }

        if (seedDto.Origin != DataOrigin.BuiltIn && seedDto.OwnerUserId is null)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' has origin '{seedDto.Origin}' but no OwnerUserId.");
        }

        var duplicateBlockSequence = seedDto.Blocks
            .GroupBy(block => block.Sequence)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateBlockSequence is not null)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' has duplicate block sequence '{duplicateBlockSequence.Key}'.");
        }
    }

    private Workout CreateWorkout(
        WorkoutSeedJsonDto seedDto,
        Description description)
    {
        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn when seedDto.Id.HasValue => Workout.NewBuiltInWithId(
                id: WorkoutId.FromGuid(seedDto.Id.Value),
                name: seedDto.Name,
                description: description,
                createdBy: SeedUser),

            DataOrigin.BuiltIn => Workout.NewBuiltIn(
                name: seedDto.Name,
                description: description,
                createdBy: SeedUser),

            _ => throw new NotSupportedException(
                $"JSON workout seeding currently only supports {nameof(DataOrigin.BuiltIn)} workouts. Workout seed '{seedDto.Name}' has origin '{seedDto.Origin}'.")
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