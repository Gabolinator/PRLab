using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Domain.Model.Value.Prescription;

public sealed record WorkoutScorePrescription
{
    public WorkoutScoreType PrimaryScoreType { get; init; }

    public IReadOnlyCollection<WorkoutScoreType> SecondaryScoreTypes { get; init; } = [];

    public bool AllowTieBreak { get; init; }

    public bool TrackScaling { get; init; }
}