using PRLab.Domain.Utilities;

namespace PRLab.Domain.Value.Identifier;

public record struct ExerciseBlockId(Guid Value)
{
    public static ExerciseBlockId New() => new(CoreUtilities.GuidGenerator.New());
    
    public static ExerciseBlockId FromGuid(Guid id) => new(id);
    
    public static ExerciseBlockId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ExerciseBlockId id) => id.Value;
    public static explicit operator ExerciseBlockId(Guid value) => new(value);
}