using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Join;

public sealed class MuscleAlias
{
    public MuscleId MuscleId { get; private set; }

    public AliasId AliasId { get; private set; }

    public string AliasValueKey { get; private set; } = string.Empty;

    public Alias Alias { get; private set; } = null!;

    private MuscleAlias()
    {
    }

    private MuscleAlias(MuscleId ownerId, Alias alias)
    {
        MuscleId = ownerId;
        AliasId = alias.Id;
        AliasValueKey = alias.ValueKey;
        Alias = alias;
    }

    internal static MuscleAlias New(MuscleId ownerId, Alias alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        DomainGuard.NotEmptyId(ownerId.Value, nameof(ownerId));
        DomainGuard.NotEmptyId(alias.Id.Value, nameof(alias));

        return new MuscleAlias(ownerId, alias);
    }
}
