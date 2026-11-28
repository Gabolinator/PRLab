using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Identifier;

public readonly record struct AliasId(Guid Value)
{
    public static AliasId New() => new(CoreUtilities.GuidGenerator.New());

    public static AliasId FromGuid(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
