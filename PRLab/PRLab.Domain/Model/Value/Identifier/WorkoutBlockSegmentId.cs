using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public record struct WorkoutBlockSegmentId(Guid Value)
{
    public static WorkoutBlockSegmentId New() => new(CoreUtilities.GuidGenerator.New());
    
    public static WorkoutBlockSegmentId FromGuid(Guid id) => new(id);
    
    public static WorkoutBlockSegmentId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(WorkoutBlockSegmentId id) => id.Value;
    public static explicit operator WorkoutBlockSegmentId(Guid value) => new(value);
}