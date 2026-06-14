using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations;

public sealed class MuscleAntagonistSeedRelationResolver
    : IMuscleAntagonistSeedRelationResolver
{
    public IReadOnlyList<SeedRelationItem<MuscleId>> Resolve(
        IReadOnlyCollection<MuscleSeedJsonDto> seedDtos,
        MuscleSeedCatalog muscleCatalog)
    {
        var relations = new List<SeedRelationItem<MuscleId>>();

        foreach (var seedDto in seedDtos)
        {
            if (seedDto.Antagonists.Count == 0)
            {
                continue;
            }

            var sourceMuscle = ResolveMuscleFromSeedDto(seedDto, muscleCatalog);

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

        return SeedRelationDeduplicator.DeduplicateUndirected(relations);
    }

    private static Muscle ResolveMuscleFromSeedDto(
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
        SeedEntityReferenceJsonDto reference,
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
}