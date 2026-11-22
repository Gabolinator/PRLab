using PRLab.Domain.Model.Value.Enum.Prescription.Time;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutSegmentImportDraft
{
    public required string Name { get; init; }

    public WorkMode? WorkMode { get; init; }

    public WorkoutScoreType? ScoreType { get; init; }

    public int? TimeCapSeconds { get; init; }

    public int? WorkWindowSeconds { get; init; }

    public int? IntervalSeconds { get; init; }

    public IntervalScope? IntervalScope { get; init; }

    public int? RestAfterSegmentSeconds { get; init; }

    public IReadOnlyList<WorkoutStepImportDraft> Steps { get; init; } = [];
}