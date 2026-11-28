using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Join;

public sealed class ExerciseAlias
{
    public ExerciseId ExerciseId { get; private set; }

    public AliasId AliasId { get; private set; }

    public string AliasValueKey { get; private set; } = string.Empty;

    public Alias Alias { get; private set; } = null!;

    private ExerciseAlias()
    {
    }

    private ExerciseAlias(ExerciseId ownerId, Alias alias)
    {
        ExerciseId = ownerId;
        AliasId = alias.Id;
        AliasValueKey = alias.ValueKey;
        Alias = alias;
    }

    internal static ExerciseAlias New(ExerciseId ownerId, Alias alias)
    {
        ArgumentNullException.ThrowIfNull(alias);
        DomainGuard.NotEmptyId(ownerId.Value, nameof(ownerId));
        DomainGuard.NotEmptyId(alias.Id.Value, nameof(alias));

        return new ExerciseAlias(ownerId, alias);
    }
}
