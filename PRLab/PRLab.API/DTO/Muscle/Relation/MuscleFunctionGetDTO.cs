using PRLab.Domain.Model.Value.Enum.Anatomy;

namespace PRLab.API.DTO.Muscle.Relation;

public sealed record MuscleFunctionGetDTO(
    MuscleFunction Function,
    MuscleFunctionRole Role);