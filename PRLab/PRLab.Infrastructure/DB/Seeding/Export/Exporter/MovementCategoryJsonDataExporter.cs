using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public class MovementCategoryJsonDataExporter(PRLabPgDBContext db, ISeedingConfig config)
    : BaseJsonSeedDataExporter<MovementCategorySeedJsonDto>(config)
{
    public override EntityType Entity => EntityType.MovementCategory;
    protected override async Task<IReadOnlyList<MovementCategorySeedJsonDto>> CreateSeedDtosAsync(CancellationToken ct)
    {
        var movementCategoryCatalog = await SeedCatalogBuilder.CreateMovementCategoryCatalog(
            db,
            ct);

        return movementCategoryCatalog
            .GetAll()
            .OrderBy(movementCategory => movementCategory.Name)
            .Select(MovementCategorySeedJsonDto.FromMovementCategory)
            .ToList();
        
        // var MovementCategorys = await db.MovementCategories
        //     .AsNoTracking()
        //     .Include(MovementCategory => MovementCategory.Description)
        //     .ThenInclude(description => description.Translations)
        //     .OrderBy(MovementCategory => MovementCategory.Name)
        //     .ToListAsync(ct);
        //
        // return MovementCategorys
        //     .Select(MovementCategorySeedJsonDto.FromMovementCategory)
        //     .ToList();
    }
}