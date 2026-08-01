using PRLab.Domain.Model.Value.Enum.Anatomy;

namespace PRLab.Domain.Model.Value.Update;

public sealed record MuscleFunctionDefinition(
    MuscleFunction Function,
    MuscleFunctionRole Role);