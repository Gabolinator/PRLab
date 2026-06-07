namespace PRLab.Infrastructure.DB.Seeding.FromJson.JsonDtos.Muscle;

public sealed record MuscleSeedReferenceJsonDto
{
    public Guid? Id { get; init; }

    public string? Name { get; init; }

    public string? NameKey { get; init; }
}