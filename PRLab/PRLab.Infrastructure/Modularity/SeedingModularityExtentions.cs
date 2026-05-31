using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Infrastructure.DB.Seeding;
using PRLab.Infrastructure.DB.Seeding.EntitySeeders;
using PRLab.Infrastructure.DB.Seeding.Factory;

namespace PRLab.Infrastructure.Modularity;

public static class SeedingModularityExtentions
{
    public static void AddSeedDataFactory(this IServiceCollection services)
    {
        services.AddScoped<IEquipmentSeedFactory, DevelopmentEquipmentSeedFactory>();
        services.AddScoped<IMuscleSeedFactory, DevelopmentMuscleSeedFactory>();
        services.AddScoped<IMovementCategorySeedFactory, DevelopmentMovementCategorySeedFactory>();
        services.AddScoped<IMuscleAntagonistSeedFactory, DevelopmentMuscleAntagonistSeedFactory>();
    }

    public static void AddDataSeeder(this IServiceCollection services)
    {
        services.AddSeedDataFactory();
        
        services.AddScoped<IEntitySeeder, EquipmentSeeder>();
        services.AddScoped<IEntitySeeder, MuscleSeeder>();
        services.AddScoped<IEntitySeeder, MuscleAntagonistSeeder>();
        services.AddScoped<IEntitySeeder, MovementCategorySeeder>();
        
        services.AddScoped<IDataSeeder, DevelopmentDataSeeder>();
    }
}