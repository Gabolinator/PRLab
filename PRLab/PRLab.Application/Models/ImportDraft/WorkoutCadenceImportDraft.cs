namespace PRLab.Application.Models.ImportDraft;

[Flags]
public enum WorkoutCadenceType
{
    None = 0,
    FixedInterval = 1,
    SelfPaced = 2
}

public sealed record WorkoutCadenceImportDraft
{
    public required WorkoutCadenceType Type { get; init; }

    public int? IntervalSeconds { get; init; }

    public static WorkoutCadenceImportDraft? Create(int? intervalSeconds, bool selfPaced)
    {
        var type = intervalSeconds.HasValue ? WorkoutCadenceType.FixedInterval : WorkoutCadenceType.None;
        if (selfPaced)
        {
            type |= WorkoutCadenceType.SelfPaced;
        }

        return type == WorkoutCadenceType.None
            ? null
            : new WorkoutCadenceImportDraft { Type = type, IntervalSeconds = intervalSeconds };
    }
}
