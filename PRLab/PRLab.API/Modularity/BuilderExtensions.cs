

using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.Modularity;


namespace PRLab.API.Modularity;

public static class BuilderExtensions
{
    public static void ConfigureEnvironment(this WebApplicationBuilder builder, IAppLogger logger)
    {
        // TODO: remove or make configurable before shipping to production.
        builder.Environment.EnvironmentName = Environments.Development;
        builder.Configuration.AddUserSecrets<Program>(optional: true);
       
        logger.Log($"ENV: {builder.Environment.EnvironmentName}");
     
    }
    
    
   public static void ConfigureServices(this WebApplicationBuilder builder, IAppLogger logger, IClock clock)
   {
       IServiceCollection services = builder.Services;
       IConfiguration configuration =  builder.Configuration;

       services.AddUtilities(clock, logger)
           .AddInfrastructure(configuration, logger, true)
           .AddSwagger()
           .AddControllers();
   }
}