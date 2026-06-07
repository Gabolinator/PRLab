using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;

public sealed record MuscleAntagonistSeedJsonDto
{
    public SeedEntityReferenceJsonDto Source { get; init; } = new();

    public SeedEntityReferenceJsonDto Target { get; init; } = new();

    public SeedAction Action { get; init; } = SeedAction.Ignore;
}