using PRLab.Domain;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record RepExecutionDetailsGetDTO(
    int? EccentricSeconds,
    int? BottomPauseSeconds,
    int? ConcentricSeconds,
    int? TopPauseSeconds,
    DomainEnum.RepPhaseExecutionIntent? EccentricIntent,
    DomainEnum.RepPhaseExecutionIntent? BottomIntent,
    DomainEnum.RepPhaseExecutionIntent? ConcentricIntent,
    DomainEnum.RepPhaseExecutionIntent? TopIntent,
    string? Intent);

public sealed record RepExecutionDetailsPostDTO
{
    public int? EccentricSeconds { get; init; }

    public int? BottomPauseSeconds { get; init; }

    public int? ConcentricSeconds { get; init; }

    public int? TopPauseSeconds { get; init; }

    public DomainEnum.RepPhaseExecutionIntent? EccentricIntent { get; init; }

    public DomainEnum.RepPhaseExecutionIntent? BottomIntent { get; init; }

    public DomainEnum.RepPhaseExecutionIntent? ConcentricIntent { get; init; }

    public DomainEnum.RepPhaseExecutionIntent? TopIntent { get; init; }

    public string? Intent { get; init; }
}

public sealed record RepExecutionDetailsPutDTO
{
    public int? EccentricSeconds { get; init; }

    public int? BottomPauseSeconds { get; init; }

    public int? ConcentricSeconds { get; init; }

    public int? TopPauseSeconds { get; init; }

    public DomainEnum.RepPhaseExecutionIntent? EccentricIntent { get; init; }

    public DomainEnum.RepPhaseExecutionIntent? BottomIntent { get; init; }

    public DomainEnum.RepPhaseExecutionIntent? ConcentricIntent { get; init; }

    public DomainEnum.RepPhaseExecutionIntent? TopIntent { get; init; }

    public string? Intent { get; init; }
}