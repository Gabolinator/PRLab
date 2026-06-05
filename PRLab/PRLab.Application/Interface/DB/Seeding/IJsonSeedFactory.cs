using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Application.Interface.DB.Seeding;

public interface IJsonSeedFactory<TEntity, TJsonDto>
{
    IReadOnlyList<TJsonDto> LoadSeedDtos();

    public SeedItem<TEntity> ToSeedItem(TJsonDto seedDto);
}