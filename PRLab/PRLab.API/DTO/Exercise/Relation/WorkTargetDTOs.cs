using PRLab.Domain;
using PRLab.Domain.Value.Enum.Prescription;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record WorkTargetGetDTO(
    decimal Value,
    WorkTargetType TargetType);

public sealed record WorkTargetPostDTO
{
    public required decimal Value { get; init; }

    public required WorkTargetType TargetType { get; init; }
}

public sealed record WorkTargetPutDTO
{
    public required decimal Value { get; init; }

    public required WorkTargetType TargetType { get; init; }
}