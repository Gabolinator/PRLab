using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public record struct WorkoutBlockSegmentStepId(Guid Value)
{
    public static WorkoutBlockSegmentStepId New() => new(CoreUtilities.GuidGenerator.New());
    
    public static WorkoutBlockSegmentStepId FromGuid(Guid id) => new(id);
    
    public static WorkoutBlockSegmentStepId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(WorkoutBlockSegmentStepId id) => id.Value;
    public static explicit operator WorkoutBlockSegmentStepId(Guid value) => new(value);
}