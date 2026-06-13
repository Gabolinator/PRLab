using PRLab.Domain;
using PRLab.Domain.Value.Enum.Prescription;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record LoadTargetGetDTO(
    decimal? Value,
    LoadTargetType Type,
    LoadUnit? Unit);

public sealed record LoadTargetPostDTO
{
    public decimal? Value { get; init; }

    public required LoadTargetType Type { get; init; }

    public LoadUnit? Unit { get; init; }
}

public sealed record LoadTargetPutDTO
{
    public decimal? Value { get; init; }

    public required LoadTargetType Type { get; init; }

    public LoadUnit? Unit { get; init; }
}