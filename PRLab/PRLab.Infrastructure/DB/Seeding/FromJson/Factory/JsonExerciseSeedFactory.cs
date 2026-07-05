using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Exercise;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Exercise;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;
using PRLab.Infrastructure.DB.Seeding.Validation;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonExerciseSeedFactory(
    IUserService userService,
    ISeedingConfig config,
    IExerciseSeedRelationResolver relationResolver)
    : BaseJsonSeedFactory<Exercise, ExerciseSeedJsonDto>(userService, config), IExerciseSeedFactory
{
    protected override EntityType Entity => EntityType.Exercise;

    public override SeedItem<Exercise> ToSeedItem(SeedExecutionOptions options, ExerciseSeedJsonDto seedDto)
    {
        throw new NotSupportedException(
            "Exercise seeds require a movement catalog. Use CreateInitialData(...catalog).");
    }

    public override void Validate(ExerciseSeedJsonDto seedDto)
    => ExerciseSeedValidator.Validate(seedDto);

    public IReadOnlyList<SeedItem<Exercise>> CreateInitialData(
        SeedExecutionOptions options,
        MovementSeedCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var seedDtos = LoadSeedDtos();

        return seedDtos
            .Select(seedDto => ToSeedItem(options,seedDto, catalog))
            .ToList();
    }

    private SeedItem<Exercise> ToSeedItem(
        SeedExecutionOptions options,
        ExerciseSeedJsonDto seedDto,
        MovementSeedCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalog);

        Validate(seedDto);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var exercise = CreateExercise(
            options,
            seedDto,
            description);

        relationResolver.ApplyRelations(
            exercise,
            seedDto,
            catalog,
            options,
            SeedUser);

        return new SeedItem<Exercise>(
            SeedKeyGenerator.GenerateExerciseKey(exercise),
            exercise,
            options.ResolveAction(seedDto.Action));
    }
    
    private Exercise CreateExercise(
        SeedExecutionOptions options,
        ExerciseSeedJsonDto seedDto,
        Description description)
    {
        var shouldUseSeedId =
            seedDto.Id.HasValue &&
            !options.IgnoreTopLevelIds;

        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn when shouldUseSeedId => Exercise.NewBuiltInWithId(
                id: ExerciseId.FromGuid(seedDto.Id!.Value),
                name: seedDto.Name,
                description: description,
                createdBy: SeedUser,
                visibilityScope: seedDto.VisibilityScope),

            DataOrigin.BuiltIn => Exercise.NewBuiltIn(
                name: seedDto.Name,
                description: description,
                createdBy: SeedUser,
                visibilityScope: seedDto.VisibilityScope),

            _ => throw new NotSupportedException(
                $"JSON exercise seeding currently only supports {nameof(DataOrigin.BuiltIn)} exercises. " +
                $"Exercise seed '{seedDto.Name}' has origin '{seedDto.Origin}'.")
        };
    }
}