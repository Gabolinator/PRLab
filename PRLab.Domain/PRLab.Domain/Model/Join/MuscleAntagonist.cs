using PRLab.Model.Entity;
using PRLab.Value.Identifier;

namespace PRLab.Model.Join;

public sealed class MuscleAntagonist
{
    public MuscleId MuscleId { get; private init; }

    public Muscle Muscle { get; private init; } = null!;

    public MuscleId AntagonistMuscleId { get; private init; }

    public Muscle AntagonistMuscle { get; private init; } = null!;

    private MuscleAntagonist()
    {
        // EF Core
    }

    private MuscleAntagonist(
        MuscleId muscleId,
        MuscleId antagonistMuscleId)
    {
        if (muscleId == antagonistMuscleId)
        {
            throw new ArgumentException("A muscle cannot be its own antagonist.");
        }

        MuscleId = muscleId;
        AntagonistMuscleId = antagonistMuscleId;
    }

    public static MuscleAntagonist New(
        MuscleId muscleId,
        MuscleId antagonistMuscleId)
    {
        return new MuscleAntagonist(muscleId, antagonistMuscleId);
    }
}