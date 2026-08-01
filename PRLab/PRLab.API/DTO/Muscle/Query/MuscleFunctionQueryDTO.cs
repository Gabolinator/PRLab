using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Utilities;

namespace PRLab.API.DTO.Muscle.Query;

public sealed record MuscleFunctionQueryDTO
{
    public MuscleFunction[] Functions { get; init; } = [];

    public MuscleFunctionRole[] Roles { get; init; } = [];

    public MuscleFunctionMatchMode MatchMode { get; init; } =
        MuscleFunctionMatchMode.Any;

    public LocalizationHelper.Language? Language { get; init; }
}