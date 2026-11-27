using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;

using PRLab.Application.WorkoutImport.Model;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutStepImportDraft
{
    public required string SourceText { get; init; }

    public string? ExerciseName { get; init; }

    public WorkoutStepKind StepKind { get; init; }

    public decimal? WorkValue { get; init; }

    public WorkTargetType? WorkTargetType { get; init; }

    public WorkTargetScope Scope { get; init; } = WorkTargetScope.Total;

    public decimal? LoadValue { get; init; }

    public LoadUnit? LoadUnit { get; init; }

    public int? RestSeconds { get; init; }

    public int? MinimumRestSeconds { get; init; }

    public int? SetCount { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyList<WorkoutImportAttributeAssignment> Attributes { get; init; } = [];

    public decimal Confidence { get; init; }
    
    public int? Order { get; init; }
}
