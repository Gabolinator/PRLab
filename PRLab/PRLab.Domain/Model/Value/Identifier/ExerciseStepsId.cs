using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public record struct ExerciseStepsId(Guid Value)
{
    public static ExerciseStepsId New() => new(CoreUtilities.GuidGenerator.New());
    
    public static ExerciseStepsId FromGuid(Guid id) => new(id);
    
    public static ExerciseStepsId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ExerciseStepsId id) => id.Value;
    public static explicit operator ExerciseStepsId(Guid value) => new(value);
}