using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory;

public sealed class DevelopmentMovementCategorySeedFactory(IUserService userService) : IMovementCategorySeedFactory
{
    private User SeedUser => userService.GetSystemAdminUser("Seed");

    public IReadOnlyList<SeedItem<MovementCategory>> CreateInitialData()
    {
        var movementCategories = new List<MovementCategory>();

        var bodyweightCategory = MovementCategory.NewBuiltIn(
            "Bodyweight",
            DomainEnum.BaseMovementCategory.BodyWeight,
            Description.New("Movements performed mainly with body weight."),
            SeedUser
        );

        var weightliftingCategory = MovementCategory.NewBuiltIn(
            "Weightlifting",
            DomainEnum.BaseMovementCategory.Resistance,
            Description.New("Movements performed with external loads."),
            SeedUser
        );

        movementCategories.Add(bodyweightCategory);
        movementCategories.Add(weightliftingCategory);

        return movementCategories
            .Select(movementCategory =>
                new SeedItem<MovementCategory>(
                    SeedKeyGenerator.GenerateMovementCategoryKey(movementCategory),
                    movementCategory,
                    SeedAction.CreateIfMissing))
            .ToList();
    }
}