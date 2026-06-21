using PRLab.Domain.Model.Value.Enum.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Time;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record TimeConstraintSeedJsonDto
{
    public TimeConstraintKind Kind { get; init; }

    public TimeSpan? Duration { get; init; }

    public static TimeConstraintSeedJsonDto? FromTimeConstraint(
        TimeConstraint? timeConstraint)
    {
        if (timeConstraint is null)
        {
            return null;
        }

        return new TimeConstraintSeedJsonDto
        {
            Kind = timeConstraint.Kind,
            Duration = timeConstraint.Duration
        };
    }
}