using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Seeding.FromJson.JsonDtos;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public class MovementCategoryJsonDataExporter(PRLabPgDBContext db, ISeedingConfig config)
    : BaseJsonSeedDataExporter<MovementCategorySeedJsonDto>(config)
{
    public override DomainEnum.EntityType Entity => DomainEnum.EntityType.MovementCategory;
    protected override async Task<IReadOnlyList<MovementCategorySeedJsonDto>> CreateSeedDtosAsync(CancellationToken ct)
    {
        var equipments = await db.MovementCategories
            .AsNoTracking()
            .Include(equipment => equipment.Description)
            .ThenInclude(description => description.Translations)
            .OrderBy(equipment => equipment.Name)
            .ToListAsync(ct);

        return equipments
            .Select(MovementCategorySeedJsonDto.FromMovementCategory)
            .ToList();
    }
}