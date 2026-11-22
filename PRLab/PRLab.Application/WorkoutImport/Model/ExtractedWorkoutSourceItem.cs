namespace PRLab.Application.WorkoutImport.Model;

public sealed record ExtractedWorkoutSourceItem
{
    public int Sequence { get; init; }

    public string? Label { get; init; }

    public required string Value { get; init; }

    public string? SourceReference { get; init; }

    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();
}