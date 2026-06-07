using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.JsonDtos.Muscle;

public sealed record MuscleAntagonistSeedJsonDto
{
    public MuscleSeedReferenceJsonDto Source { get; init; } = new();

    public MuscleSeedReferenceJsonDto Target { get; init; } = new();

    public SeedAction Action { get; init; } = SeedAction.Ignore;
}