using PRLab.Domain.Model.Value.Enum.Prescription.Load;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutLoadImportDraft
{
    public required decimal Value { get; init; }

    public required LoadUnit Unit { get; init; }

    public static WorkoutLoadImportDraft FromSemantic(WorkoutLoadSemantic load) =>
        new() { Value = load.Value, Unit = load.Unit };
}
