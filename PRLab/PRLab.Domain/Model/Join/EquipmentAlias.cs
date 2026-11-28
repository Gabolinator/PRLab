using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Join;

public sealed class EquipmentAlias
{
    public EquipmentId EquipmentId { get; private set; }

    public AliasId AliasId { get; private set; }

    public string AliasValueKey { get; private set; } = string.Empty;

    public Alias Alias { get; private set; } = null!;

    private EquipmentAlias()
    {
    }

    private EquipmentAlias(EquipmentId ownerId, Alias alias)
    {
        EquipmentId = ownerId;
        AliasId = alias.Id;
        AliasValueKey = alias.ValueKey;
        Alias = alias;
    }

    internal static EquipmentAlias New(EquipmentId ownerId, Alias alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        DomainGuard.NotEmptyId(ownerId.Value, nameof(ownerId));
        DomainGuard.NotEmptyId(alias.Id.Value, nameof(alias));

        return new EquipmentAlias(ownerId, alias);
    }
}
