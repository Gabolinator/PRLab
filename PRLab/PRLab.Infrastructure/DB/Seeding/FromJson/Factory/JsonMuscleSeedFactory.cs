using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding;
using PRLab.Infrastructure.DB.Seeding.FromJson;
using PRLab.Infrastructure.DB.Seeding.FromJson.Factory;
using PRLab.Infrastructure.DB.Seeding.FromJson.JsonDtos.Muscle;

public sealed class JsonMuscleSeedFactory(
    IUserService userService,
    ISeedingConfig config)
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

        var relations = new List<SeedRelationItem<MuscleId>>();

        foreach (var seedDto in seedDtos)
        {
            if (seedDto.Antagonists.Count == 0)
            {
                continue;
            }

            var sourceMuscle = ResolveMuscleFromSourceSeedDto(seedDto, muscleCatalog);

            foreach (var antagonistRef in seedDto.Antagonists)
            {
                var antagonistMuscle = ResolveMuscleReference(
                    antagonistRef,
                    muscleCatalog);

                relations.Add(
                    new SeedRelationItem<MuscleId>(
                        sourceMuscle.Id,
                        antagonistMuscle.Id,
                        SeedAction.CreateIfMissing));
            }
        }

        return DeduplicateRelations(relations);
    }

    private static Muscle ResolveMuscleFromSourceSeedDto(
        MuscleSeedJsonDto seedDto,
        MuscleSeedCatalog muscleCatalog)
    {
        if (seedDto.Id.HasValue)
        {
            return muscleCatalog.GetRequiredById(
                MuscleId.FromGuid(seedDto.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(seedDto.NameKey))
        {
            return muscleCatalog.GetRequiredByNameKey(seedDto.NameKey);
        }

        return muscleCatalog.GetRequiredByName(seedDto.Name);
    }

    private static Muscle ResolveMuscleReference(
        MuscleSeedReferenceJsonDto reference,
        MuscleSeedCatalog muscleCatalog)
    {
        if (reference.Id.HasValue)
        {
            return muscleCatalog.GetRequiredById(
                MuscleId.FromGuid(reference.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(reference.NameKey))
        {
            return muscleCatalog.GetRequiredByNameKey(reference.NameKey);
        }

        if (!string.IsNullOrWhiteSpace(reference.Name))
        {
            return muscleCatalog.GetRequiredByName(reference.Name);
        }

        throw new InvalidOperationException(
            "Muscle antagonist reference must provide Id, NameKey, or Name.");
    }

    private static IReadOnlyList<SeedRelationItem<MuscleId>> DeduplicateRelations(
        IReadOnlyCollection<SeedRelationItem<MuscleId>> relations)
    {
        var seen = new HashSet<string>();
        var result = new List<SeedRelationItem<MuscleId>>();

        foreach (var relation in relations)
        {
            var left = relation.SourceId.Value;
            var right = relation.TargetId.Value;

            var key = left.CompareTo(right) <= 0
                ? $"{left}:{right}"
                : $"{right}:{left}";

            if (!seen.Add(key))
            {
                continue;
            }

            result.Add(relation);
        }

        return result;
    }
}