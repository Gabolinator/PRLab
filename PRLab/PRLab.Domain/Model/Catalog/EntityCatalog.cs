namespace PRLab.Domain.Model.Catalog;

public sealed class EntityCatalog<TId, TEntity>
{
    private readonly IReadOnlyDictionary<TId, TEntity> entitiesById;
    private readonly IReadOnlyDictionary<string, TEntity> entitiesByNameKey;

    public EntityCatalog(
        IEnumerable<TEntity> entities,
        Func<TEntity, TId> idSelector,
        Func<TEntity, string> nameKeySelector)
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(idSelector);
        ArgumentNullException.ThrowIfNull(nameKeySelector);

        var entityList = entities.ToList();

        entitiesById = entityList.ToDictionary(idSelector);

        entitiesByNameKey = entityList.ToDictionary(
            nameKeySelector,
            entity => entity);
    }

    public bool TryGetById(TId id, out TEntity? entity)
    {
        return entitiesById.TryGetValue(id, out entity);
    }

    public bool TryGetByNameKey(string nameKey, out TEntity? entity)
    {
        if (string.IsNullOrWhiteSpace(nameKey))
        {
            entity = default;
            return false;
        }

        return entitiesByNameKey.TryGetValue(nameKey, out entity);
    }

    public TEntity GetRequiredById(TId id)
    {
        if (entitiesById.TryGetValue(id, out var entity))
        {
            return entity;
        }

        throw new InvalidOperationException(
            $"Entity with id '{id}' was not found in catalog.");
    }

    public TEntity GetRequiredByNameKey(string nameKey)
    {
        if (TryGetByNameKey(nameKey, out var entity) && entity is not null)
        {
            return entity;
        }

        throw new InvalidOperationException(
            $"Entity with name key '{nameKey}' was not found in catalog.");
    }
}