using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory;

public sealed class DevelopmentEquipmentSeedFactory(IUserService userService) : IEquipmentSeedFactory
{
    private User SeedUser => userService.GetSystemAdminUser("Seed");

    public IReadOnlyList<SeedItem<Equipment>> CreateInitialData()
    {
        var barbell = Equipment.NewBuiltIn(
            "Barbell",
            Description.New("Long metal bar used for loaded strength training."),
            SeedUser
        );

        var dumbbell = Equipment.NewBuiltIn(
            "Dumbbell",
            Description.New("Short handheld weight used for unilateral or bilateral exercises."),
            SeedUser
        );

        var pullUpBar = Equipment.NewBuiltIn(
            "Pull-up Bar",
            Description.New("Fixed or mounted bar used for pull-ups, hangs, and other bodyweight pulling movements."),
            SeedUser
        );

        var jumpRope = Equipment.NewBuiltIn(
            "Jump Rope",
            Description.New("Rope used for jumping movements, conditioning, coordination, and footwork drills."),
            SeedUser
        );

        return
        [
            new SeedItem<Equipment>(
                SeedKeyGenerator.GenerateEquipmentKey(barbell),
                barbell,
                SeedAction.CreateIfMissing),

            new SeedItem<Equipment>(
                SeedKeyGenerator.GenerateEquipmentKey(dumbbell),
                dumbbell,
                SeedAction.CreateIfMissing),

            new SeedItem<Equipment>(
                SeedKeyGenerator.GenerateEquipmentKey(pullUpBar),
                pullUpBar,
                SeedAction.CreateIfMissing),

            new SeedItem<Equipment>(
                SeedKeyGenerator.GenerateEquipmentKey(jumpRope),
                jumpRope,
                SeedAction.CreateIfMissing),
        ];
    }
}