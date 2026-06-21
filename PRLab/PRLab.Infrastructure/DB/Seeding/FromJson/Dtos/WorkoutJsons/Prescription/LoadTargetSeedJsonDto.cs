using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Load;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record LoadTargetSeedJsonDto
{
    public decimal? Value { get; init; }

    public LoadTargetType Type { get; init; }

    public LoadUnit? Unit { get; init; }

    public int? ReferenceRepMax { get; init; }

    public LoadReferenceSeedJsonDto? LoadReference { get; init; }

    public static LoadTargetSeedJsonDto? FromLoadTarget(LoadTarget? loadTarget)
    {
        if (loadTarget is null)
        {
            return null;
        }

        return new LoadTargetSeedJsonDto
        {
            Value = loadTarget.Value,
            Type = loadTarget.Type,
            Unit = loadTarget.Unit,
            ReferenceRepMax = loadTarget.ReferenceRepMax,
            LoadReference = LoadReferenceSeedJsonDto.FromLoadReference(
                loadTarget.LoadReference)
        };
    }
}