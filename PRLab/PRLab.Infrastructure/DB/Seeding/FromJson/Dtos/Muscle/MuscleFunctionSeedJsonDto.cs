using PRLab.Domain.Model.Value.Enum.Anatomy;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;

public sealed record MuscleFunctionSeedJsonDto
{
    public MuscleFunction Function { get; init; }

    public MuscleFunctionRole Role { get; init; }

    public static MuscleFunctionSeedJsonDto FromMuscleFunction(
        Domain.Model.Join.MuscleFunctionAssignment muscleFunction)
    {
        ArgumentNullException.ThrowIfNull(muscleFunction);

        return new MuscleFunctionSeedJsonDto
        {
            Function = muscleFunction.Function,
            Role = muscleFunction.Role,
        };
    }
}