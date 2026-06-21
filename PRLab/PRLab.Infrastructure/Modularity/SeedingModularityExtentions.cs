using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Export;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Exercise;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Movement;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Muscle;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Workout;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Seeding.Config;
using PRLab.Infrastructure.DB.Seeding.Development.Factory;
using PRLab.Infrastructure.DB.Seeding.Development.Factory.ExerciseFactory;
using PRLab.Infrastructure.DB.Seeding.Development.Factory.MovementFactory;
using PRLab.Infrastructure.DB.Seeding.Development.Factory.MuscleFactory;
using PRLab.Infrastructure.DB.Seeding.Development.Factory.WorkoutFactory;
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

        var options = seedingOptions ?? new SeedingOptions();
        
        logger.Log(@$"Seeding is Enabled : {options.SeedingEnabled} - to: {options.SeedFileDirectory}");
        
        return options.SeedingEnabled 
            ? services
            .AddSeedingConfig(options)
            .AddSeedDataFactories(options, logger)
            .AddDataSeeders(options)
            .AddSeedExporters()
            : services;
    }

    private static IServiceCollection AddSeedRelationResolver(
        this IServiceCollection services,
        IAppLogger logger)
    {
        return services
            .AddScoped<IMuscleAntagonistSeedRelationResolver, MuscleAntagonistSeedRelationResolver>()
            .AddScoped<IMovementSeedRelationResolver, MovementSeedRelationResolver>()
            .AddScoped<IExerciseSeedRelationResolver, ExerciseSeedRelationResolver>()
            .AddScoped<IWorkoutSeedRelationResolver, WorkoutSeedRelationResolver>();

    }

    private static IServiceCollection AddSeedDataFactories(
        this IServiceCollection services,
        SeedingOptions options,
        IAppLogger logger)
    {
        logger.Log($"Adding Seeding Factory: {options.Source}");
        
        return options.Source switch
        {
            SeedingSource.JsonFiles => services.AddSeedRelationResolver(logger)
                .AddScoped<IEquipmentSeedFactory, JsonEquipmentSeedFactory>()
                .AddScoped<IMovementCategorySeedFactory, JsonMovementCategorySeedFactory>()
                .AddScoped<IMuscleSeedFactory, JsonMuscleSeedFactory>()
                .AddScoped<IMuscleAntagonistSeedFactory, JsonMuscleSeedFactory>()
                .AddScoped<IMovementSeedFactory, JsonMovementSeedFactory>()
                .AddScoped<IExerciseSeedFactory, JsonExerciseSeedFactory>()
                .AddScoped<IWorkoutSeedFactory, JsonWorkoutSeedFactory>(),

            SeedingSource.Factory => services
                .AddScoped<IEquipmentSeedFactory, DevelopmentEquipmentSeedFactory>()
                .AddScoped<IMuscleSeedFactory, DevelopmentMuscleSeedFactory>()
                .AddScoped<IMovementCategorySeedFactory, DevelopmentMovementCategorySeedFactory>()
                .AddScoped<IMuscleAntagonistSeedFactory, DevelopmentMuscleAntagonistSeedFactory>()
                .AddScoped<IMovementSeedFactory, DevelopmentMovementSeedFactory>()
                .AddScoped<IExerciseSeedFactory, DevelopmentExerciseSeedFactory>()
                .AddScoped<IWorkoutSeedFactory, DevelopmentWorkoutSeedFactory>(),
            _ => services
        };
    }

    private static IServiceCollection AddDataSeeders(this IServiceCollection services, SeedingOptions options)
    {
        services.AddSeederIfFactoryExists<IEquipmentSeedFactory, EquipmentSeeder>();
        services.AddSeederIfFactoryExists<IMuscleSeedFactory, MuscleSeeder>();
        services.AddSeederIfFactoryExists<IMuscleAntagonistSeedFactory, MuscleAntagonistSeeder>();
        services.AddSeederIfFactoryExists<IMovementCategorySeedFactory, MovementCategorySeeder>();
        services.AddSeederIfFactoryExists<IMovementSeedFactory, MovementSeeder>();
        services.AddSeederIfFactoryExists<IExerciseSeedFactory, ExerciseSeeder>();
        services.AddSeederIfFactoryExists<IWorkoutSeedFactory, WorkoutSeeder>();
        
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
        SeedingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.AddSingleton<ISeedingConfig, SeedingConfig>();

        return services;
    }

    private static IServiceCollection AddSeedExporters(this IServiceCollection services)
    {
        services.AddScoped<ISeedDataExporter, EquipmentJsonDataExporter>()
            .AddScoped<ISeedDataExporter, MovementCategoryJsonDataExporter>()
            .AddScoped<ISeedDataExporter, MuscleJsonDataExporter>()
            .AddScoped<ISeedDataExporter, MovementJsonDataExporter>()
            .AddScoped<ISeedDataExporter, ExerciseJsonDataExporter>()
            .AddScoped<ISeedDataExporter, WorkoutJsonDataExporter>();
            
        services.AddScoped<ISeedDataExportOrchestrator, SeedDataExportOrchestrator>();

        return services;
    }
}