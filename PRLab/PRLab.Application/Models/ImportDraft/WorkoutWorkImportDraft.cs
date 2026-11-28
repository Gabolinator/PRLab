using PRLab.Domain.Model.Value.Enum.Prescription.Work;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutWorkImportDraft
{
    public required decimal Value { get; init; }

    public required WorkTargetType TargetType { get; init; }

    public WorkTargetScope Scope { get; init; } = WorkTargetScope.Total;

    public int? SetCount { get; init; }

    public static WorkoutWorkImportDraft FromSemantic(WorkoutWorkSemantic work) =>
        new()
        {
            Value = work.Value,
            TargetType = work.TargetType,
            Scope = work.Scope,
            SetCount = work.SetCount
        };
}
