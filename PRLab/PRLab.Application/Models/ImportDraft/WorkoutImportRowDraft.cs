using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutImportRowDraft
{
    public required string SourceText { get; init; }

    public required string SourceReference { get; init; }

    public int? Order { get; init; }

    public string? Details { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyList<WorkoutImportExercisePart> ExerciseParts { get; init; } = [];
}

public sealed record WorkoutImportExercisePart
{
    public required string SourceText { get; init; }

    public string? MatchedText { get; init; }

    public string? RemainingText { get; init; }

    public string? ExerciseName { get; init; }

    public ExerciseId? ExerciseId { get; init; }

    public string? Warning { get; init; }
}
