namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutRestImportDraft
{
    public int? Seconds { get; init; }

    public int? MinimumSeconds { get; init; }

    public static WorkoutRestImportDraft FromSemantic(WorkoutRestSemantic rest) =>
        rest.IsMinimum ? new() { MinimumSeconds = rest.Seconds } : new() { Seconds = rest.Seconds };
}
