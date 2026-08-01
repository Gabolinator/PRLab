using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Muscle;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public class MuscleJsonDataExporter(PRLabPgDBContext db, ISeedingConfig config)
    : BaseJsonSeedDataExporter<MuscleSeedJsonDto>(config)
{
    public override EntityType Entity => EntityType.Muscle;
    protected override async Task<IReadOnlyList<MuscleSeedJsonDto>> CreateSeedDtosAsync(CancellationToken ct)
    {
        var muscleCatalog = await SeedCatalogBuilder.CreateMuscleCatalog(
            db,
            ct);

        return muscleCatalog
            .GetAll()
            .OrderBy(muscle => muscle.Name)
            .Select(MuscleSeedJsonDto.FromMuscle)
            .ToList();
    }
}