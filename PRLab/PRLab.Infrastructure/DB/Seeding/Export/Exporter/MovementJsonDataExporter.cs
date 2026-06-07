using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public class MovementJsonDataExporter(
    PRLabPgDBContext db,
    ISeedingConfig config)
    : BaseJsonSeedDataExporter<MovementSeedJsonDto>(config)
{
    public override DomainEnum.EntityType Entity => DomainEnum.EntityType.Movement;

    protected override async Task<IReadOnlyList<MovementSeedJsonDto>> CreateSeedDtosAsync(
        CancellationToken ct)
    {
        var movements = await db.Movements
            .AsNoTracking()
            .AsSplitQuery()
            .Include(movement => movement.MovementCategory)
            .Include(movement => movement.Description)
                .ThenInclude(description => description.Translations)
            .Include(movement => movement.EquipmentRequirements)
                .ThenInclude(movementEquipment => movementEquipment.Equipment)
            .Include(movement => movement.Muscles)
                .ThenInclude(movementMuscle => movementMuscle.Muscle)
            .Include(movement => movement.VariantOf)
            .Include(movement => movement.Patterns)
            .OrderBy(movement => movement.Name)
            .ToListAsync(ct);

        return movements
            .Select(MovementSeedJsonDto.FromMovement)
            .ToList();
    }
}