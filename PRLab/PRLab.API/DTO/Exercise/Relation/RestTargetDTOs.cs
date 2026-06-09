namespace PRLab.API.DTO.Exercise.Relation;

public sealed record RestTargetGetDTO(
    int? Seconds);

public sealed record RestTargetPostDTO
{
    public int? Seconds { get; init; }
}

public sealed record RestTargetPutDTO
{
    public int? Seconds { get; init; }
}