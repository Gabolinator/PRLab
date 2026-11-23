using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutBlockImportDraft
{
    public required string Name { get; init; }

    public WorkoutBlockType? BlockType { get; init; }

    public int RepeatCount { get; init; } = 1;

    public int? RestBetweenRepeatsSeconds { get; init; }

    public IReadOnlyList<WorkoutSegmentImportDraft> Segments { get; init; } = [];
    
    public int? Order { get; init; }
}