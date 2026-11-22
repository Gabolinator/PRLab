using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.Modularity;
using PRLab.Tools.Commands;

namespace PRLab.Tools.Modularity;

public static class PRToolServicesExtensions
{
    public static IServiceCollection AddPRToolHandlers(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<ToolCommandHandler>()
            .AddScoped<ToolCommandUsageLogger>()
            .AddSeedHandling()
            .AddToolImportHandling(configuration);
    }

    public static IServiceCollection AddSeedHandling(this IServiceCollection services)
    {
        return services
            .AddScoped<SeedToolCommandHandler>()
            .AddScoped<ExportSeedToolCommandHandler>();
    }
    
    public static IServiceCollection AddToolImportHandling(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<ImportWorkoutCommandHandler>();
    }
    
    
    public static IServiceCollection AddPRLabToolServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment,
        IClock clock,
        IAppLogger logger )
    {
       return services
           .AddUtilities(clock, logger)
           .AddInfrastructure(configuration, isDevelopment ,logger)
           .AddPRToolHandlers(configuration);
    }
}