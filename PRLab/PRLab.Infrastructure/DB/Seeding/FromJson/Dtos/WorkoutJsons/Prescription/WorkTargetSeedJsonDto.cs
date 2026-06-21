using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record WorkTargetSeedJsonDto
{
    public decimal Value { get; init; }

    public WorkTargetType TargetType { get; init; }

    public WorkTargetScope Scope { get; init; }

    public static WorkTargetSeedJsonDto? FromWorkTarget(WorkTarget? workTarget)
    {
        if (workTarget is null)
        {
            return null;
        }

        return new WorkTargetSeedJsonDto
        {
            Value = workTarget.Value,
            TargetType = workTarget.TargetType,
            Scope = workTarget.Scope
        };
    }
}