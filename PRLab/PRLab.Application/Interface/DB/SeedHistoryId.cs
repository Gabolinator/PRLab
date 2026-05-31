using PRLab.Domain.Utilities;

namespace PRLab.Application.Interface.DB;


public record struct SeedHistoryId(Guid Value)
{
    public static SeedHistoryId New() => new(CoreUtilities.GuidGenerator.New());
    public static SeedHistoryId FromGuid(Guid id) => new(id);
    
    public static SeedHistoryId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(SeedHistoryId id) => id.Value;
    public static explicit operator SeedHistoryId(Guid value) => new(value);
}