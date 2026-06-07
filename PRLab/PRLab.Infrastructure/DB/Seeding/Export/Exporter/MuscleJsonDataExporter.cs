using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Seeding.FromJson.JsonDtos.Muscle;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public class MuscleJsonDataExporter(PRLabPgDBContext db, ISeedingConfig config)
    : BaseJsonSeedDataExporter<MuscleSeedJsonDto>(config)
{
    public override DomainEnum.EntityType Entity => DomainEnum.EntityType.Muscle;
    protected override async Task<IReadOnlyList<MuscleSeedJsonDto>> CreateSeedDtosAsync(CancellationToken ct)
    {
        var muscles = await db.Muscles
            .AsNoTracking()
            .AsSplitQuery()
            .Include(muscle => muscle.Description)
                .ThenInclude(description => description.Translations)
            .Include(muscle => muscle.Antagonists)
                .ThenInclude(antagonist => antagonist.AntagonistMuscle)
            .OrderBy(muscle => muscle.Name)
            .ToListAsync(ct);

        return muscles
            .Select(MuscleSeedJsonDto.FromMuscle)
            .ToList();
    }
}