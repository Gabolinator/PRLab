using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.Modularity;

namespace PRLab.Tools.Modularity;

public static class PRToolServicesExtensions
{
    public static IServiceCollection AddPRToolHandlers(this IServiceCollection services)
    {
       return services.AddScoped<ToolCommandHandler>()
            .AddScoped<ToolCommandUsageLogger>()
            .AddScoped<SeedToolCommandHandler>()
            .AddScoped<ExportSeedToolCommandHandler>();
       
      
    }

    public static IServiceCollection AddPRLabToolServices(this IServiceCollection services, IConfiguration configuration,
        IClock clock, IAppLogger logger )
    {
       return services.AddUtilities(clock, logger)
            .AddInfrastructure(configuration, logger)
            .AddPRToolHandlers();
    }
}