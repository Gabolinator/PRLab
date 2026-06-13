using Microsoft.Extensions.Logging;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Movement;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory.Movement;

public sealed class DevelopmentMovementSeedFactory(IUserService userService, ILogger<DevelopmentMovementSeedFactory> logger) : IMovementSeedFactory
{
    private User SeedUser => userService.GetSystemAdminUser("Seed");

    public IReadOnlyList<SeedItem<Domain.Model.Entity.Movement>> CreateInitialData(
        MovementSeedCatalogs catalogs)
    {
        var movementCategoryCatalog = catalogs.MovementCategory;
        var equipmentCatalog = catalogs.Equipment;
        var muscleCatalog = catalogs.Muscle;
        
        var bodyweightCategory = movementCategoryCatalog.GetRequiredByName("Bodyweight", logger);
        
        var pullUpBar = equipmentCatalog.GetRequiredByName("Pull-up Bar", logger);
        var jumpRope = equipmentCatalog.GetRequiredByName("Jump Rope", logger);

        var chest = muscleCatalog.GetRequiredByName("Chest", logger);
        var lats = muscleCatalog.GetRequiredByName("Lats", logger);
        var frontDelts = muscleCatalog.GetRequiredByName("Front Delts", logger);
        var biceps = muscleCatalog.GetRequiredByName("Biceps", logger);
        var triceps = muscleCatalog.GetRequiredByName("Triceps", logger);
        var abs = muscleCatalog.GetRequiredByName("Abs", logger);
        var quads = muscleCatalog.GetRequiredByName("Quads", logger);
        var hamstrings = muscleCatalog.GetRequiredByName("Hamstrings", logger);
        var glutes = muscleCatalog.GetRequiredByName("Glutes", logger);
        var calves = muscleCatalog.GetRequiredByName("Calves", logger);

        var bodyweightSquat = CreateMovement(
            "Bodyweight Squat",
            bodyweightCategory,
            "Basic lower-body squat movement performed without external load.");

        bodyweightSquat.AddPrimaryMuscle(quads.Id, SeedUser);
        bodyweightSquat.AddSecondaryMuscle(glutes.Id, SeedUser);
        bodyweightSquat.AddSecondaryMuscle(hamstrings.Id, SeedUser);
        bodyweightSquat.AddPattern(Domain.DomainEnum.MovementPattern.Squat);
        
        var pushUp = CreateMovement(
            "Push Up",
            bodyweightCategory,
            "Bodyweight upper-body pushing movement performed from the floor.");

        pushUp.AddPrimaryMuscle(chest.Id, SeedUser);
        pushUp.AddSecondaryMuscle(triceps.Id, SeedUser);
        pushUp.AddSecondaryMuscle(frontDelts.Id, SeedUser);
        pushUp.AddSecondaryMuscle(abs.Id, SeedUser);
        pushUp.AddPattern(DomainEnum.MovementPattern.Push);

        var pullUp = CreateMovement(
            "Pull Up",
            bodyweightCategory,
            "Bodyweight upper-body pulling movement performed from a bar.");

        pullUp.AddPrimaryMuscle(lats.Id, SeedUser);
        pullUp.AddSecondaryMuscle(biceps.Id, SeedUser);
        pullUp.AddSecondaryMuscle(abs.Id, SeedUser);
        pullUp.AddRequiredEquipmentOption(pullUpBar.Id, "bar" ,SeedUser);
        pullUp.AddPattern(DomainEnum.MovementPattern.Pull);
        
        var doubleUnder = CreateMovement(
            "Double Under",
            bodyweightCategory,
            "Jump rope movement where the rope passes under the feet twice per jump.");

        doubleUnder.AddPrimaryMuscle(calves.Id, SeedUser);
        doubleUnder.AddSecondaryMuscle(quads.Id, SeedUser);
        doubleUnder.AddSecondaryMuscle(abs.Id, SeedUser);
        doubleUnder.AddRequiredEquipmentOption(jumpRope.Id, "rope" ,SeedUser);
        doubleUnder.AddPattern(DomainEnum.MovementPattern.Jump);
        
       
        var movements = new List<Domain.Model.Entity.Movement>
        {
            bodyweightSquat,
            pushUp,
            pullUp,
            doubleUnder,
        };

        return movements
            .Select(movement =>
                new SeedItem<Domain.Model.Entity.Movement>(
                    SeedKeyGenerator.GenerateMovementKey(movement),
                    movement,
                    SeedAction.CreateIfMissing))
            .ToList();
    }

    private Domain.Model.Entity.Movement CreateMovement(
        string name,
        MovementCategory movementCategory,
        string description)
    {
        return Domain.Model.Entity.Movement.NewBuiltIn(
            name,
            movementCategory,
            Description.New(description),
            SeedUser);
    }
}