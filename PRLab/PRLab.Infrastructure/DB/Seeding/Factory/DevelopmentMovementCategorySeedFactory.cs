using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Infrastructure.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.Factory;

public sealed class DevelopmentMovementCategorySeedFactory(IUserService userService) : IMovementCategorySeedFactory
{
    private User SeedUser => userService.GetAdminUser("Seed");
    
    public IReadOnlyList<SeedItem<MovementCategory>> CreateInitialData()
    {
        var movementCategories = new List<MovementCategory>();

        var bodyweightCategory = MovementCategory.New(
            "Bodyweight",
            Description.New("Movements performed mainly with body weight."),
            DomainEnum.BaseMovementCategory.BodyWeight,
            SeedUser
        );

        var weightliftingCategory = MovementCategory.New(
            "Weightlifting",
            Description.New("Movements performed with external loads."),
            DomainEnum.BaseMovementCategory.Resistance,
            SeedUser
        );

        movementCategories.Add(bodyweightCategory);
        movementCategories.Add(weightliftingCategory);

        return movementCategories
            .Select(movementCategory =>
                new SeedItem<MovementCategory>(
                    SeedKeyGenerator.GenerateMovementCategoryKey(movementCategory),
                    movementCategory))
            .ToList();
    }
}