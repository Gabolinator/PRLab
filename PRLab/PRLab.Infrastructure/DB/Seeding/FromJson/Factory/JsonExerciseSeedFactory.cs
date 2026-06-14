using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Exercise;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Exercise;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonExerciseSeedFactory(
    IUserService userService,
    ISeedingConfig config,
    IExerciseSeedRelationResolver relationResolver)
    : BaseJsonSeedFactory<Exercise, ExerciseSeedJsonDto>(userService, config), IExerciseSeedFactory
{
    protected override EntityType Entity => EntityType.Exercise;

    public override SeedItem<Exercise> ToSeedItem(ExerciseSeedJsonDto seedDto)
    {
        throw new NotSupportedException(
            "Exercise seeds require a movement catalog. Use CreateInitialData(...catalog).");
    }

    public IReadOnlyList<SeedItem<Exercise>> CreateInitialData(
        MovementSeedCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var seedDtos = LoadSeedDtos();

        return seedDtos
            .Select(seedDto => ToSeedItem(seedDto, catalog))
            .ToList();
    }

    private SeedItem<Exercise> ToSeedItem(
        ExerciseSeedJsonDto seedDto,
        MovementSeedCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalog);

        ValidateSeedDto(seedDto);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var exercise = CreateExercise(
            seedDto,
            description);

        relationResolver.ApplyRelations(
            exercise,
            seedDto,
            catalog,
            SeedUser);

        return new SeedItem<Exercise>(
            SeedKeyGenerator.GenerateExerciseKey(exercise),
            exercise,
            seedDto.Action);
    }

    private static void ValidateSeedDto(
        ExerciseSeedJsonDto seedDto)
    {
        if (string.IsNullOrWhiteSpace(seedDto.Name))
        {
            throw new InvalidOperationException("Exercise seed name cannot be empty.");
        }

        if (seedDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{seedDto.Name}' has an empty id. Omit the Id property or provide a valid id.");
        }

        if (seedDto.Origin == DataOrigin.BuiltIn && seedDto.OwnerUserId is not null)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{seedDto.Name}' is BuiltIn and should not provide OwnerUserId.");
        }

        if (seedDto.Origin != DataOrigin.BuiltIn && seedDto.OwnerUserId is null)
        {
            throw new InvalidOperationException(
                $"Exercise seed '{seedDto.Name}' has origin '{seedDto.Origin}' but no OwnerUserId.");
        }
    }

    private Exercise CreateExercise(
        ExerciseSeedJsonDto seedDto,
        Description description)
    {
        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn when seedDto.Id.HasValue => Exercise.NewBuiltInWithId(
                id: ExerciseId.FromGuid(seedDto.Id.Value),
                name: seedDto.Name,
                description: description,
                createdBy: SeedUser),

            DataOrigin.BuiltIn => Exercise.NewBuiltIn(
                name: seedDto.Name,
                description: description,
                createdBy: SeedUser),

            _ => throw new NotSupportedException(
                $"JSON exercise seeding currently only supports {nameof(DataOrigin.BuiltIn)} exercises. Exercise seed '{seedDto.Name}' has origin '{seedDto.Origin}'.")
        };
    }
}