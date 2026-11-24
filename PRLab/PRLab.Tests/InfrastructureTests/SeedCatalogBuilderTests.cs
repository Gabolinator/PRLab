using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Infrastructure.DB.Helpers;

namespace PRLab.Tests.InfrastructureTests;

public sealed class SeedCatalogBuilderTests
{
    [Fact]
    public async Task TrackedMovementCategoryCatalog_CanAddMovementWithAnAlreadyTrackedCategory()
    {
        await using var database = await RepositoryTestDatabase.CreateAsync();
        var db = database.Db;
        var category = MovementCategory.NewBuiltIn(
            "Strength Training",
            "Strength movements.",
            BaseMovementCategory.Resistance);

        db.MovementCategories.Add(category);
        await db.SaveChangesAsync();

        var catalog = await SeedCatalogBuilder.CreateMovementCategoryCatalog(
            db, CancellationToken.None, track: true);
        var resolved = catalog.GetRequiredByNameKey(category.NameKey);
        var movement = Movement.NewBuiltIn(
            "Shoulder Press", resolved, Description.None(),
            WorkTargetType.Repetitions, MovementLaterality.Bilateral);

        await db.Movements.AddAsync(movement);
        await db.SaveChangesAsync();

        Assert.Same(category, resolved);
        Assert.Equal(category.Id, await db.Movements
            .Where(item => item.Id == movement.Id)
            .Select(item => item.MovementCategoryId)
            .SingleAsync());
    }
}
