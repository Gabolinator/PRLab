using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Model.Value.Enum.Anatomy;

namespace PRLab.API.DTO.Muscle.Relation;

public sealed record MuscleFunctionInputDTO
{
    [EnumDataType(typeof(MuscleFunction))]
    public MuscleFunction Function { get; init; }

    [EnumDataType(typeof(MuscleFunctionRole))]
    public MuscleFunctionRole Role { get; init; }
}