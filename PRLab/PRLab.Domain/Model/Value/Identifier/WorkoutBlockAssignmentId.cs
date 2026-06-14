using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public record struct WorkoutBlockAssignmentId(Guid Value)
{
    public static WorkoutBlockAssignmentId New() => new(CoreUtilities.GuidGenerator.New());
    
    public static WorkoutBlockAssignmentId FromGuid(Guid id) => new(id);
    
    public static WorkoutBlockAssignmentId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(WorkoutBlockAssignmentId id) => id.Value;
    public static explicit operator WorkoutBlockAssignmentId(Guid value) => new(value);
}