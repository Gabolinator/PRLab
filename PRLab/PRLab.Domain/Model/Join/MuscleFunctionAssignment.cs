using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Join;

public sealed record MuscleFunctionAssignment
{
    public MuscleId MuscleId { get; init; }

    public Muscle Muscle { get; private set; } = null!;

    public MuscleFunction Function { get; init; }

    public MuscleFunctionRole Role { get; private set; }

    private MuscleFunctionAssignment()
    {
        // EF Core
    }

    private MuscleFunctionAssignment(
        MuscleId muscleId,
        MuscleFunction function,
        MuscleFunctionRole role)
    {
        if (muscleId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Muscle id cannot be empty.",
                nameof(muscleId));
        }

        if (!Enum.IsDefined(function))
        {
            throw new ArgumentOutOfRangeException(nameof(function));
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        MuscleId = muscleId;
        Function = function;
        Role = role;
    }

    public static MuscleFunctionAssignment New(
        MuscleId muscleId,
        MuscleFunction function,
        MuscleFunctionRole role)
    {
        return new MuscleFunctionAssignment(
            muscleId,
            function,
            role);
    }

    public bool ChangeRole(MuscleFunctionRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        if (Role == role)
        {
            return false;
        }

        Role = role;

        return true;
    }
}