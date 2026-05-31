using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.Factory;

public sealed class DevelopmentEquipmentSeedFactory(IUserService userService) : IEquipmentSeedFactory
{
    private User SeedUser => userService.GetAdminUser("Seed");

    public IReadOnlyList<SeedItem<Equipment>> CreateInitialData()
    {
        var barbell = Equipment.New(
            "Barbell",
            Description.New("Long metal bar used for loaded strength training."),
            SeedUser
        );

        var dumbbell = Equipment.New(
            "Dumbbell",
            Description.New("Short handheld weight used for unilateral or bilateral exercises."),
            SeedUser
        );

        var jumpRope = Equipment.New(
            "Jump Rope",
            Description.None(),
            SeedUser
        );

        return
        [
            new SeedItem<Equipment>(
                SeedKeyGenerator.GenerateEquipmentKey(barbell),
                barbell,
                SeedAction.CreateOrUpdate),

            new SeedItem<Equipment>(
                SeedKeyGenerator.GenerateEquipmentKey(dumbbell),
                dumbbell,
                SeedAction.CreateOrUpdate),

            new SeedItem<Equipment>(
                SeedKeyGenerator.GenerateEquipmentKey(jumpRope),
                jumpRope,
                SeedAction.CreateOrUpdate),
        ];
    }
}