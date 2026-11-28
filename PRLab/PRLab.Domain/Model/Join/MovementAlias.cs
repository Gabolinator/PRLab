using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Join;

public sealed class MovementAlias
{
    public MovementId MovementId { get; private set; }

    public AliasId AliasId { get; private set; }

    public string AliasValueKey { get; private set; } = string.Empty;

    public Alias Alias { get; private set; } = null!;

    private MovementAlias()
    {
    }

    private MovementAlias(MovementId ownerId, Alias alias)
    {
        MovementId = ownerId;
        AliasId = alias.Id;
        AliasValueKey = alias.ValueKey;
        Alias = alias;
    }

    internal static MovementAlias New(MovementId ownerId, Alias alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        DomainGuard.NotEmptyId(ownerId.Value, nameof(ownerId));
        DomainGuard.NotEmptyId(alias.Id.Value, nameof(alias));

        return new MovementAlias(ownerId, alias);
    }
}
