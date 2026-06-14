using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public record struct ExerciseId(Guid Value)
{
        public static ExerciseId New() => new(CoreUtilities.GuidGenerator.New());
        public override string ToString() => Value.ToString();
    
        public static ExerciseId FromGuid(Guid id) => new(id);
    
        public static ExerciseId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;

        public static implicit operator Guid(ExerciseId id) => id.Value;
        public static explicit operator ExerciseId(Guid value) => new(value);
}
