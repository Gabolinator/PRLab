using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonMuscleSeedFactory(
    IUserService userService,
    ISeedingConfig config,
    IMuscleAntagonistSeedRelationResolver antagonistRelationResolver)
    : BaseJsonSeedFactory<Muscle, MuscleSeedJsonDto>(userService, config),
        IMuscleSeedFactory,
        IMuscleAntagonistSeedFactory
{
    protected override DomainEnum.EntityType Entity => DomainEnum.EntityType.Muscle;

    public IReadOnlyList<SeedItem<Muscle>> CreateInitialData()
    {
        return CreateSeedItems();
    }

    public override SeedItem<Muscle> ToSeedItem(MuscleSeedJsonDto seedDto)
    {
        if (string.IsNullOrWhiteSpace(seedDto.Name))
        {
            throw new InvalidOperationException($"{Entity} seed name cannot be empty.");
        }

        if (seedDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"{Entity} seed '{seedDto.Name}' has an empty id. Omit the Id property or provide a valid id.");
        }

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var muscle = seedDto.Id.HasValue
            ? Muscle.NewWithId(
                MuscleId.FromGuid(seedDto.Id.Value),
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
            seedDto.Action);
    }

    public IReadOnlyList<SeedRelationItem<MuscleId>> CreateInitialData(
        MuscleSeedCatalog muscleCatalog)
    {
        var seedDtos = LoadSeedDtos();

        return antagonistRelationResolver.Resolve(
            seedDtos,
            muscleCatalog);
    }
}