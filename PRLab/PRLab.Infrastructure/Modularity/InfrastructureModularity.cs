using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Config;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.WorkoutImport;
using PRLab.Infrastructure.WorkoutImport.Extractor;

namespace PRLab.Infrastructure.Modularity;

public static class InfrastructureModularity
{
    public static IServiceCollection AddUtilities(this IServiceCollection services, IClock clock, IAppLogger logger)
    {
        return services.AddSingleton<IAppLogger>(logger)
            .AddSingleton<IClock>(clock);
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment,
        IAppLogger logger)

    {
        var updatedServices = services
            .AddDBContextConfiguration(configuration, logger)
            .AddRepositories()
            .AddUserServices(isDevelopment, configuration)
            .AddDataSeeding(configuration, logger)
            .AddWorkoutImport(configuration);
        
        return updatedServices;
    }
    
  

}