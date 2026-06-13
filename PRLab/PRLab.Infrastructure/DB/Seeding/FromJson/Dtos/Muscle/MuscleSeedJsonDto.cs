using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Value.Enum.Anatomy;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;

public sealed record MuscleSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public string? LatinName { get; init; }

    public BodySection BodySection { get; init; }

    public DescriptionSeedJsonDto? Description { get; init; }

    public IReadOnlyList<SeedEntityReferenceJsonDto> Antagonists { get; init; } = [];

    public SeedAction Action { get; init; } = SeedAction.Ignore;

    public static MuscleSeedJsonDto FromMuscle(Domain.Model.Entity.Muscle muscle)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        return new MuscleSeedJsonDto
        {
            Id = muscle.Id.Value,
            Name = muscle.Name,
            NameKey = muscle.NameKey,
            LatinName = muscle.LatinName,
            BodySection = muscle.BodySection,
            Description = muscle.Description is null
                ? null
                : DescriptionSeedJsonDto.FromDescription(muscle.Description),
            Antagonists = muscle.Antagonists
                .Select(antagonist => new SeedEntityReferenceJsonDto
                {
                    Id = antagonist.AntagonistMuscleId.Value,
                    Name = antagonist.AntagonistMuscle.Name,
                    NameKey = antagonist.AntagonistMuscle.NameKey,
                })
                .ToList(),
            Action = SeedAction.Ignore,
        };
    }
}