using Microsoft.Extensions.Logging;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Movement;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory.MovementFactory;

public sealed class DevelopmentMovementSeedFactory(
    IUserService userService,
    ILogger<DevelopmentMovementSeedFactory> logger) : IMovementSeedFactory
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
            name: "Bodyweight Squat",
            movementCategory: bodyweightCategory,
            defaultTargetType: WorkTargetType.Repetitions,
            allowedTargetTypes:
            [
                WorkTargetType.Repetitions,
                WorkTargetType.TimeUnderTensionSeconds
            ],
            description: "Basic lower-body squat movement performed without external load.");

        bodyweightSquat.AddPrimaryMuscle(quads.Id, SeedUser);
        bodyweightSquat.AddSecondaryMuscle(glutes.Id, SeedUser);
        bodyweightSquat.AddSecondaryMuscle(hamstrings.Id, SeedUser);
        bodyweightSquat.AddPattern(MovementPattern.Squat);

        var pushUp = CreateMovement(
            name: "Push Up",
            movementCategory: bodyweightCategory,
            defaultTargetType: WorkTargetType.Repetitions,
            allowedTargetTypes:
            [
                WorkTargetType.Repetitions,
                WorkTargetType.TimeUnderTensionSeconds
            ],
            description: "Bodyweight upper-body pushing movement performed from the floor.");

        pushUp.AddPrimaryMuscle(chest.Id, SeedUser);
        pushUp.AddSecondaryMuscle(triceps.Id, SeedUser);
        pushUp.AddSecondaryMuscle(frontDelts.Id, SeedUser);
        pushUp.AddSecondaryMuscle(abs.Id, SeedUser);
        pushUp.AddPattern(MovementPattern.Push);

        var pullUp = CreateMovement(
            name: "Pull Up",
            movementCategory: bodyweightCategory,
            defaultTargetType: WorkTargetType.Repetitions,
            allowedTargetTypes:
            [
                WorkTargetType.Repetitions,
                WorkTargetType.TimeUnderTensionSeconds
            ],
            description: "Bodyweight upper-body pulling movement performed from a bar.");

        pullUp.AddPrimaryMuscle(lats.Id, SeedUser);
        pullUp.AddSecondaryMuscle(biceps.Id, SeedUser);
        pullUp.AddSecondaryMuscle(abs.Id, SeedUser);
        pullUp.AddRequiredEquipmentOption(pullUpBar.Id, "bar", SeedUser);
        pullUp.AddPattern(MovementPattern.Pull);

        var doubleUnder = CreateMovement(
            name: "Double Under",
            movementCategory: bodyweightCategory,
            defaultTargetType: WorkTargetType.Repetitions,
            allowedTargetTypes:
            [
                WorkTargetType.Repetitions,
                WorkTargetType.DurationSeconds,
                WorkTargetType.Calories
            ],
            description: "Jump rope movement where the rope passes under the feet twice per jump.");

        doubleUnder.AddPrimaryMuscle(calves.Id, SeedUser);
        doubleUnder.AddSecondaryMuscle(quads.Id, SeedUser);
        doubleUnder.AddSecondaryMuscle(abs.Id, SeedUser);
        doubleUnder.AddRequiredEquipmentOption(jumpRope.Id, "rope", SeedUser);
        doubleUnder.AddPattern(MovementPattern.Jump);

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
        WorkTargetType defaultTargetType,
        IReadOnlyCollection<WorkTargetType> allowedTargetTypes,
        string description)
    {
        return Domain.Model.Entity.Movement.NewBuiltIn(
            name: name,
            movementCategory: movementCategory,
            description: Description.New(description),
            defaultWorkTargetType: defaultTargetType,
            allowedWorkTargetTypes: allowedTargetTypes,
            createdBy: SeedUser);
    }
}