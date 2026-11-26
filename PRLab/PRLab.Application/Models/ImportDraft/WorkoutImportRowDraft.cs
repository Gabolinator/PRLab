using PRLab.Domain.Model.Value.Identifier;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutImportRowDraft
{
    public required string SourceText { get; init; }

    public required string SourceReference { get; init; }

    public int? Order { get; init; }

    public string? Details { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyList<WorkoutImportExercisePart> ExerciseParts { get; init; } = [];

    public IReadOnlyList<WorkoutImportPrescriptionDraft> Prescriptions { get; init; } = [];

    public IReadOnlyList<WorkoutWorkWindowDraft> WorkWindows { get; init; } = [];

    public IReadOnlyList<WorkoutImportTextFragment> UnconsumedText { get; init; } = [];
}

public sealed record WorkoutImportPrescriptionDraft(
    string? SourceReference, WorkoutStepImportDraft Step, int? TimeCapSeconds = null);

public sealed record WorkoutWorkWindowDraft(
    WorkMode Mode, int DurationSeconds, string SourceText, string? SourceReference);

public sealed record WorkoutImportTextFragment(string? SourceReference, string Text);

public sealed record WorkoutImportExercisePart
{
    public required string SourceText { get; init; }

    public string? MatchedText { get; init; }

    public string? RemainingText { get; init; }

    public string? ExerciseName { get; init; }

    public ExerciseId? ExerciseId { get; init; }

    public IReadOnlyList<WorkoutImportAttributeAssignment> Attributes { get; init; } = [];

    public string? Warning { get; init; }
}
