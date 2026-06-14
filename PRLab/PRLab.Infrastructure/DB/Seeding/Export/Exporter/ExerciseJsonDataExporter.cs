using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Helpers;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Exercise;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public class ExerciseJsonDataExporter(
    PRLabPgDBContext db,
    ISeedingConfig config)
    : BaseJsonSeedDataExporter<ExerciseSeedJsonDto>(config)
{
    public override EntityType Entity => EntityType.Exercise;

    protected override async Task<IReadOnlyList<ExerciseSeedJsonDto>> CreateSeedDtosAsync(
        CancellationToken ct)
    {
        var exerciseCatalog = await SeedCatalogBuilder.CreateExerciseCatalog(db, ct);

        return exerciseCatalog
            .GetAll()
            .OrderBy(exercise => exercise.Name)
            .Select(ExerciseSeedJsonDto.FromExercise)
            .ToList();
    }
}
