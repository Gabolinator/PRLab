
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Value.Identifier;

public record struct DescriptionId(Guid Value)
{
    public static DescriptionId New() => new(CoreUtilities.GuidGenerator.New());
    public static DescriptionId FromGuid(Guid id) => new(id);
    
    public static DescriptionId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(DescriptionId id) => id.Value;
    public static explicit operator DescriptionId(Guid value) => new(value);
}