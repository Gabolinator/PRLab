using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public record struct WorkoutBlockId(Guid Value)
{
    public static WorkoutBlockId New() => new(CoreUtilities.GuidGenerator.New());
    
    public static WorkoutBlockId FromGuid(Guid id) => new(id);
    
    public static WorkoutBlockId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(WorkoutBlockId id) => id.Value;
    public static explicit operator WorkoutBlockId(Guid value) => new(value);
}