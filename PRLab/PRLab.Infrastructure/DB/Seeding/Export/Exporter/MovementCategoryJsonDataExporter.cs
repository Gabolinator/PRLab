using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Value.Enum.System;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public class MovementCategoryJsonDataExporter(PRLabPgDBContext db, ISeedingConfig config)
    : BaseJsonSeedDataExporter<MovementCategorySeedJsonDto>(config)
{
    public override EntityType Entity => EntityType.MovementCategory;
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