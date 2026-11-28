using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutStepImportDraft
{
    public required string SourceText { get; init; }

    public string? ExerciseName { get; init; }

    public WorkoutStepKind StepKind { get; init; }

    public WorkoutWorkImportDraft? Work { get; init; }

    public WorkoutLoadImportDraft? Load { get; init; }

    public WorkoutRestImportDraft? Rest { get; init; }

    public WorkoutCadenceImportDraft? Cadence { get; init; }

    public WorkoutExecutionImportDraft? Execution { get; init; }

    public WorkoutRateImportDraft? Rate { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyList<WorkoutImportAttributeAssignment> Attributes { get; init; } = [];

    public decimal Confidence { get; init; }
    
    public int? Order { get; init; }
}
