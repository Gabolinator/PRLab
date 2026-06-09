using PRLab.Domain;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record WorkTargetGetDTO(
    decimal Value,
    DomainEnum.WorkTargetType TargetType);

public sealed record WorkTargetPostDTO
{
    public required decimal Value { get; init; }

    public required DomainEnum.WorkTargetType TargetType { get; init; }
}

public sealed record WorkTargetPutDTO
{
    public required decimal Value { get; init; }

    public required DomainEnum.WorkTargetType TargetType { get; init; }
}