using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public record struct DescriptionTranslationId(Guid Value)
{
    public static DescriptionTranslationId New() => new(CoreUtilities.GuidGenerator.New());
    public static DescriptionTranslationId FromGuid(Guid id) => new(id);
    
    public static DescriptionTranslationId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(DescriptionTranslationId id) => id.Value;
    public static explicit operator DescriptionTranslationId(Guid value) => new(value);
}