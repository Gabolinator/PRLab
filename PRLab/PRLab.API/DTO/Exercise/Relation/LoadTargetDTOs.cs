using PRLab.Domain;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record LoadTargetGetDTO(
    decimal? Value,
    DomainEnum.LoadTargetType Type,
    DomainEnum.LoadUnit? Unit);

public sealed record LoadTargetPostDTO
{
    public decimal? Value { get; init; }

    public required DomainEnum.LoadTargetType Type { get; init; }

    public DomainEnum.LoadUnit? Unit { get; init; }
}

public sealed record LoadTargetPutDTO
{
    public decimal? Value { get; init; }

    public required DomainEnum.LoadTargetType Type { get; init; }

    public DomainEnum.LoadUnit? Unit { get; init; }
}