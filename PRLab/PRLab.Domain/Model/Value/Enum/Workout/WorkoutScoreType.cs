namespace PRLab.Domain.Model.Value.Enum.Workout;

public enum WorkoutScoreType
{
    None,
    Completed,        // Done / not done
    TimeToComplete,   // Finish time
    CompletedRoundsAndExtraReps, // AMRAP score: 7 rounds + 8 reps
    TotalReps,        // Total reps completed
    TotalLoad,        // Volume / tonnage
    Distance,         // Distance achieved
    Calories,         // Calories achieved
    MaxLoad,          // Heaviest successful load
    Quality           // Technique/quality notes or rating
}
