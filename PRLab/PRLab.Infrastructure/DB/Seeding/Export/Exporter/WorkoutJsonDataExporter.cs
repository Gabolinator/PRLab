using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public class WorkoutJsonDataExporter(
    PRLabPgDBContext db,
    ISeedingConfig config)
    : BaseJsonSeedDataExporter<WorkoutSeedJsonDto>(config)
{
    public override EntityType Entity => EntityType.Workout;
    protected override async Task<IReadOnlyList<WorkoutSeedJsonDto>> CreateSeedDtosAsync(CancellationToken ct)
    {
        var workoutCatalog = await SeedCatalogBuilder.CreateWorkoutCatalog(db, ct);

        return workoutCatalog
            .GetAll()
            .OrderBy(workout => workout.Name)
            .Select(WorkoutSeedJsonDto.FromWorkout)
            .ToList();
    }
}