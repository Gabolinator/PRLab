using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Application.Interface.DB.Seeding.Factory;

public interface IJsonSeedFactory<TEntity, TJsonDto>
{
    IReadOnlyList<TJsonDto> LoadSeedDtos();

    public SeedItem<TEntity> ToSeedItem(SeedExecutionOptions options, TJsonDto seedDto);
}