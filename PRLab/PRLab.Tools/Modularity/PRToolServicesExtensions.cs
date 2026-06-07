using Microsoft.Extensions.DependencyInjection;

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
}