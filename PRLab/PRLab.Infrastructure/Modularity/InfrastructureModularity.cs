using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.Modularity;

public static class InfrastructureModularity
{
    public static IServiceCollection AddUtilities(this IServiceCollection services, IClock clock, IAppLogger logger)
    {
        return services.AddSingleton<IAppLogger>(logger)
            .AddSingleton<IClock>(clock);
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration,
        IAppLogger logger, bool addSeeding = false)

    {
        var updatedServices = services
            .AddDBContextConfiguration(configuration, logger)
            .AddRepositories()
            .AddUserService();

        return !addSeeding ? updatedServices : updatedServices.AddDataSeeding(configuration, logger);
    }

}