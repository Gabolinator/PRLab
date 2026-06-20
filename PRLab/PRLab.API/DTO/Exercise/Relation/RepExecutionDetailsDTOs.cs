using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Repetition;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record RepExecutionDetailsGetDTO(
    int? EccentricSeconds,
    int? BottomPauseSeconds,
    int? ConcentricSeconds,
    int? TopPauseSeconds,
    RepPhaseExecutionIntent? EccentricIntent,
    RepPhaseExecutionIntent? BottomIntent,
    RepPhaseExecutionIntent? ConcentricIntent,
    RepPhaseExecutionIntent? TopIntent,
    string? Intent);

public sealed record RepExecutionDetailsPostDTO
{
    public int? EccentricSeconds { get; init; }

    public int? BottomPauseSeconds { get; init; }

    public int? ConcentricSeconds { get; init; }

    public int? TopPauseSeconds { get; init; }

    public RepPhaseExecutionIntent? EccentricIntent { get; init; }

    public RepPhaseExecutionIntent? BottomIntent { get; init; }

    public RepPhaseExecutionIntent? ConcentricIntent { get; init; }

    public RepPhaseExecutionIntent? TopIntent { get; init; }

    public string? Intent { get; init; }
}

public sealed record RepExecutionDetailsPutDTO
{
    public int? EccentricSeconds { get; init; }

    public int? BottomPauseSeconds { get; init; }

    public int? ConcentricSeconds { get; init; }

    public int? TopPauseSeconds { get; init; }

    public RepPhaseExecutionIntent? EccentricIntent { get; init; }

    public RepPhaseExecutionIntent? BottomIntent { get; init; }

    public RepPhaseExecutionIntent? ConcentricIntent { get; init; }

    public RepPhaseExecutionIntent? TopIntent { get; init; }

    public string? Intent { get; init; }
}