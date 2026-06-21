using PRLab.Domain.Model.Value.Enum.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Rest;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record RestTargetSeedJsonDto
{
    public RestPolicy Policy { get; init; }

    public int? Seconds { get; init; }

    public int? MinimumSeconds { get; init; }

    public int? MaximumSeconds { get; init; }

    public static RestTargetSeedJsonDto? FromRestTarget(RestTarget? restTarget)
    {
        if (restTarget is null)
        {
            return null;
        }

        return new RestTargetSeedJsonDto
        {
            Policy = restTarget.Policy,
            Seconds = restTarget.Seconds,
            MinimumSeconds = restTarget.MinimumSeconds,
            MaximumSeconds = restTarget.MaximumSeconds
        };
    }
}