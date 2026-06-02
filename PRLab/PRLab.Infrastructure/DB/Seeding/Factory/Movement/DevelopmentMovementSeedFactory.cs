using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.Factory.Movement;

public sealed class DevelopmentMovementSeedFactory(IUserService userService) : IMovementSeedFactory
{
    private User SeedUser => userService.GetAdminUser("Seed");

    public IReadOnlyList<SeedItem<Domain.Model.Entity.Movement>> CreateInitialData(
        EquipmentSeedCatalog equipmentCatalog,
        MuscleSeedCatalog muscleCatalog,
        MovementCategorySeedCatalog movementCategoryCatalog)
    {
        var bodyweightCategory = movementCategoryCatalog.GetRequiredByName("Bodyweight");

        var bodyweight = equipmentCatalog.GetRequiredByName("Bodyweight");
        var pullUpBar = equipmentCatalog.GetRequiredByName("Pull-up Bar");
        var jumpRope = equipmentCatalog.GetRequiredByName("Jump Rope");

        var chest = muscleCatalog.GetRequiredByName("Chest");
        var lats = muscleCatalog.GetRequiredByName("Lats");
        var frontDelts = muscleCatalog.GetRequiredByName("Front Delts");
        var biceps = muscleCatalog.GetRequiredByName("Biceps");
        var triceps = muscleCatalog.GetRequiredByName("Triceps");
        var abs = muscleCatalog.GetRequiredByName("Abs");
        var quads = muscleCatalog.GetRequiredByName("Quads");
        var hamstrings = muscleCatalog.GetRequiredByName("Hamstrings");
        var glutes = muscleCatalog.GetRequiredByName("Glutes");
        var calves = muscleCatalog.GetRequiredByName("Calves");

        var bodyweightSquat = CreateMovement(
            "Bodyweight Squat",
            bodyweightCategory,
            "Basic lower-body squat movement performed without external load.");

        bodyweightSquat.AddPrimaryMuscle(quads.Id, SeedUser);
        bodyweightSquat.AddSecondaryMuscle(glutes.Id, SeedUser);
        bodyweightSquat.AddSecondaryMuscle(hamstrings.Id, SeedUser);
        bodyweightSquat.AddEquipment(bodyweight.Id, SeedUser);

        var pushUp = CreateMovement(
            "Push Up",
            bodyweightCategory,
            "Bodyweight upper-body pushing movement performed from the floor.");

        pushUp.AddPrimaryMuscle(chest.Id, SeedUser);
        pushUp.AddSecondaryMuscle(triceps.Id, SeedUser);
        pushUp.AddSecondaryMuscle(frontDelts.Id, SeedUser);
        pushUp.AddSecondaryMuscle(abs.Id, SeedUser);
        pushUp.AddEquipment(bodyweight.Id, SeedUser);

        var pullUp = CreateMovement(
            "Pull Up",
            bodyweightCategory,
            "Bodyweight upper-body pulling movement performed from a bar.");

        pullUp.AddPrimaryMuscle(lats.Id, SeedUser);
        pullUp.AddSecondaryMuscle(biceps.Id, SeedUser);
        pullUp.AddSecondaryMuscle(abs.Id, SeedUser);
        pullUp.AddEquipment(bodyweight.Id, SeedUser);
        pullUp.AddEquipment(pullUpBar.Id, SeedUser);

        var doubleUnder = CreateMovement(
            "Double Under",
            bodyweightCategory,
            "Jump rope movement where the rope passes under the feet twice per jump.");

        doubleUnder.AddPrimaryMuscle(calves.Id, SeedUser);
        doubleUnder.AddSecondaryMuscle(quads.Id, SeedUser);
        doubleUnder.AddSecondaryMuscle(abs.Id, SeedUser);
        doubleUnder.AddEquipment(jumpRope.Id, SeedUser);

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
        return Domain.Model.Entity.Movement.New(
            name,
            movementCategory,
            Description.New(description),
            SeedUser);
    }
}