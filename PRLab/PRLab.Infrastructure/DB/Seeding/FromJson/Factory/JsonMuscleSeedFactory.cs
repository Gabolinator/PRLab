using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Muscle;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;
using PRLab.Infrastructure.DB.Seeding.Validation;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonMuscleSeedFactory(
    IUserService userService,
    ISeedingConfig config,
    IMuscleAntagonistSeedRelationResolver antagonistRelationResolver)
    : BaseJsonSeedFactory<Muscle, MuscleSeedJsonDto>(userService, config),
        IMuscleSeedFactory,
        IMuscleAntagonistSeedFactory
{
    protected override EntityType Entity => EntityType.Muscle;

    public IReadOnlyList<SeedItem<Muscle>> CreateInitialData(
        SeedExecutionOptions options)
    {
        return CreateSeedItems(options);
    }

    public override SeedItem<Muscle> ToSeedItem(
        SeedExecutionOptions options,
        MuscleSeedJsonDto seedDto)
    {
        ArgumentNullException.ThrowIfNull(options);

        Validate(seedDto);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var shouldUseSeedId =
            seedDto.Id.HasValue &&
            !options.IgnoreTopLevelIds;

        var muscle = shouldUseSeedId
            ? Muscle.NewWithId(
                MuscleId.FromGuid(seedDto.Id!.Value),
                seedDto.Name,
                seedDto.LatinName,
                seedDto.BodySection,
                description,
                SeedUser)
            : Muscle.New(
                seedDto.Name,
                seedDto.LatinName,
                seedDto.BodySection,
                description,
                SeedUser);

        return new SeedItem<Muscle>(
            SeedKeyGenerator.GenerateMuscleKey(muscle),
            muscle,
            options.ResolveAction(seedDto.Action));
    }

    public override void Validate(MuscleSeedJsonDto seedDto)
        => MuscleSeedValidator.Validate(seedDto);

    public IReadOnlyList<SeedRelationItem<MuscleId>> CreateInitialData(
        SeedExecutionOptions options,
        MuscleSeedCatalog muscleCatalog)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(muscleCatalog);

        var seedDtos = LoadSeedDtos();

        return antagonistRelationResolver.Resolve(
            options,
            seedDtos,
            muscleCatalog);
    }
}