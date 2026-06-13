using Microsoft.Extensions.Logging;
using PRLab.Domain.Model.Catalog;
using PRLab.Domain.Utilities;

namespace PRLab.Application.Models.DB.Seeding.Catalog;

public abstract class BaseSeedCatalog<TId, TEntity>(EntityCatalog<TId, TEntity> catalog)
{
    protected EntityCatalog<TId, TEntity> Catalog { get; } = catalog;
    
    public virtual TEntity GetRequiredByName(string name, ILogger? logger = null)
    {
        var namekey = FormatingUtilities.NormalizeNameKey(name);
        logger?.LogInformation($"Getting entity {name}- with name key {namekey}");
        
        return Catalog.GetRequiredByNameKey(namekey);
    }

    public virtual TEntity GetRequiredByNameKey(string namekey, ILogger? logger = null)
    {
        logger?.LogInformation($"Getting entity name key {namekey}");
        return Catalog.GetRequiredByNameKey(namekey);
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

    public IReadOnlyCollection<TEntity> GetAll()
    {
        return Catalog.GetAll();
    }
}