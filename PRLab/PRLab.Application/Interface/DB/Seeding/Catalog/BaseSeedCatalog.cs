using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Utilities;

namespace PRLab.Application.Interface.DB.Seeding.Catalog;

public abstract class BaseSeedCatalog<TId, TEntity>(EntityCatalog<TId, TEntity> catalog)
{
    protected EntityCatalog<TId, TEntity> Catalog { get; } = catalog;
    
    public virtual TEntity GetRequiredByName(string name)
    {
        return Catalog.GetRequiredByNameKey(
            FormatingUtilities.NormalizeNameKey(name));
    }

    public virtual bool TryGetByName(string name, out TEntity? entity)
    {
        return Catalog.TryGetByNameKey(
            FormatingUtilities.NormalizeNameKey(name),
            out entity);
    }

    public virtual TEntity GetRequiredById(TId id)
    {
        return Catalog.GetRequiredById(id);
    }
}