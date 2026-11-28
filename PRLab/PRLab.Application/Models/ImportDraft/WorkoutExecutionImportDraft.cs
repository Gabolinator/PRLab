namespace PRLab.Application.Models.ImportDraft;

public enum WorkoutExecutionScope
{
    Exercise,
    Rep,
    Set
}

public sealed record WorkoutExecutionImportDraft
{
    public WorkoutHoldImportDraft? Hold { get; init; }

    public WorkoutPauseImportDraft? Pause { get; init; }

    public static WorkoutExecutionImportDraft? Create(WorkoutHoldImportDraft? hold, WorkoutPauseImportDraft? pause) =>
        hold is null && pause is null ? null : new WorkoutExecutionImportDraft { Hold = hold, Pause = pause };
}

public sealed record WorkoutHoldImportDraft
{
    public required int DurationSeconds { get; init; }

    public required WorkoutExecutionScope Scope { get; init; }

    public WorkoutPausePosition? Position { get; init; }

    public static WorkoutHoldImportDraft FromSemantic(WorkoutHoldSemantic hold) =>
        new() { DurationSeconds = hold.SecondsPerRep, Scope = WorkoutExecutionScope.Rep };
}

public sealed record WorkoutPauseImportDraft
{
    public int? DurationSeconds { get; init; }

    public WorkoutExecutionScope Scope { get; init; } = WorkoutExecutionScope.Exercise;

    public WorkoutPausePosition? Position { get; init; }

    public static WorkoutPauseImportDraft FromSemantic(WorkoutPauseSemantic pause) =>
        new() { Position = pause.Position };
}
