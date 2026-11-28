namespace PRLab.Application.Models.ImportDraft;

public enum WorkoutRateMetric
{
    Calories,
    Meters,
    Repetitions,
    Watts
}

public enum WorkoutRatePeriod
{
    Second,
    Minute,
    Hour
}

public sealed record WorkoutRateImportDraft
{
    public required decimal Value { get; init; }

    public required WorkoutRateMetric Metric { get; init; }

    public required WorkoutRatePeriod Period { get; init; }

    public static WorkoutRateImportDraft FromSemantic(WorkoutCalorieRateSemantic rate) =>
        new() { Value = rate.CaloriesPerHour, Metric = WorkoutRateMetric.Calories, Period = WorkoutRatePeriod.Hour };
}
