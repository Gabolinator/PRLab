using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Export;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Seeding.Config;
using PRLab.Infrastructure.DB.Seeding.Development.Factory;
using PRLab.Infrastructure.DB.Seeding.Development.Factory.Movement;
using PRLab.Infrastructure.DB.Seeding.Development.Factory.Muscle;
using PRLab.Infrastructure.DB.Seeding.EntitySeeders;
using PRLab.Infrastructure.DB.Seeding.Export;
using PRLab.Infrastructure.DB.Seeding.Export.Exporter;
using PRLab.Infrastructure.DB.Seeding.FromJson.Factory;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.Modularity;

public static class SeedingModularityExtensions
{
    public static IServiceCollection AddDataSeeding(
        this IServiceCollection services,
        IConfiguration configuration,
        IAppLogger logger)
    {
        var seedingOptions = configuration
            .GetSection("Seeding")
            .Get<SeedingOptions>();
        
        return services
            .AddSeedingConfig(configuration)
            .AddSeedDataFactories(seedingOptions ?? new SeedingOptions(), logger)
            .AddDataSeeders()
            .AddSeedExporters();
    }

    private static IServiceCollection AddSeedRelationResolver(
        this IServiceCollection services,
        IAppLogger logger)
    {
        return services
            .AddScoped<IMuscleAntagonistSeedRelationResolver, MuscleAntagonistSeedRelationResolver>()
            .AddScoped<IMovementSeedRelationResolver, MovementSeedRelationResolver>();

    }

    private static IServiceCollection AddSeedDataFactories(
        this IServiceCollection services,
        SeedingOptions options,
        IAppLogger logger)
    {
        if (options.SeedFromJsonFile)
        {
            logger.Log("Seeding From Json files");
            return services.AddSeedRelationResolver(logger)
                .AddScoped<IEquipmentSeedFactory, JsonEquipmentSeedFactory>()
                .AddScoped<IMovementCategorySeedFactory, JsonMovementCategorySeedFactory>()
                .AddScoped<IMuscleSeedFactory, JsonMuscleSeedFactory>()
                .AddScoped<IMuscleAntagonistSeedFactory, JsonMuscleSeedFactory>()
                .AddScoped<IMovementSeedFactory, JsonMovementSeedFactory>();
        }

        
        logger.Log("Seeding from Factories ");
        return services
            .AddScoped<IEquipmentSeedFactory, DevelopmentEquipmentSeedFactory>()
            .AddScoped<IMuscleSeedFactory, DevelopmentMuscleSeedFactory>()
            .AddScoped<IMovementCategorySeedFactory, DevelopmentMovementCategorySeedFactory>()
            .AddScoped<IMuscleAntagonistSeedFactory, DevelopmentMuscleAntagonistSeedFactory>()
            .AddScoped<IMovementSeedFactory, DevelopmentMovementSeedFactory>();
    }

    private static IServiceCollection AddDataSeeders(this IServiceCollection services)
    {
        services.AddSeederIfFactoryExists<IEquipmentSeedFactory, EquipmentSeeder>();
        services.AddSeederIfFactoryExists<IMuscleSeedFactory, MuscleSeeder>();
        services.AddSeederIfFactoryExists<IMuscleAntagonistSeedFactory, MuscleAntagonistSeeder>();
        services.AddSeederIfFactoryExists<IMovementCategorySeedFactory, MovementCategorySeeder>();
        services.AddSeederIfFactoryExists<IMovementSeedFactory, MovementSeeder>();
        
        services.AddScoped<IDataSeeder, EntityDataSeeder>();

        return services;
    }

    private static IServiceCollection AddSeederIfFactoryExists<TFactory, TSeeder>(
        this IServiceCollection services)
        where TFactory : class
        where TSeeder : class, IEntitySeeder
    {
        var factoryIsRegistered = services.Any(service =>
            service.ServiceType == typeof(TFactory));

        if (!factoryIsRegistered)
        {
            return services;
        }

        services.AddScoped<IEntitySeeder, TSeeder>();

        return services;
    }

    private static IServiceCollection AddSeedingConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SeedingOptions>(
            configuration.GetSection("Seeding"));

        services.AddSingleton<ISeedingConfig, SeedingConfig>();

        return services;
    }

    private static IServiceCollection AddSeedExporters(this IServiceCollection services)
    {
        services.AddScoped<ISeedDataExporter, EquipmentJsonDataExporter>()
            .AddScoped<ISeedDataExporter, MovementCategoryJsonDataExporter>()
            .AddScoped<ISeedDataExporter, MuscleJsonDataExporter>()
            .AddScoped<ISeedDataExporter, MovementJsonDataExporter>()
            .AddScoped<ISeedDataExportOrchestrator, SeedDataExportOrchestrator>();

        return services;
    }
}