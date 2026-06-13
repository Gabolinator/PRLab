using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Value.Enum.System;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Seeding.FromJson;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public sealed class EquipmentJsonDataExporter(PRLabPgDBContext db, ISeedingConfig config)
    : BaseJsonSeedDataExporter<EquipmentSeedJsonDto>(config)
{
   public override EntityType Entity => EntityType.Equipment;

    protected override async Task<IReadOnlyList<EquipmentSeedJsonDto>> CreateSeedDtosAsync(CancellationToken ct)
    {
        var equipments = await db.Equipments
            .AsNoTracking()
            .Include(equipment => equipment.Description)
            .ThenInclude(description => description.Translations)
            .OrderBy(equipment => equipment.Name)
            .ToListAsync(ct);

        return equipments
            .Select(EquipmentSeedJsonDto.FromEquipment)
            .ToList();
    }
}