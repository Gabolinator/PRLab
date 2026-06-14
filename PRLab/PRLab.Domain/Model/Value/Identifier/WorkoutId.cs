using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public record struct WorkoutId(Guid Value)
{
 public static WorkoutId New() => new(CoreUtilities.GuidGenerator.New());
 public override string ToString() => Value.ToString();
    
 public static WorkoutId FromGuid(Guid id) => new(id);
    
 public static WorkoutId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;

 public static implicit operator Guid(WorkoutId id) => id.Value;
 public static explicit operator WorkoutId(Guid value) => new(value);
}
