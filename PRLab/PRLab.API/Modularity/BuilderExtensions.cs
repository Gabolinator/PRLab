

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
        
       services.AddSingleton<IAppLogger>(logger);
       services.AddSingleton<IClock>(clock);

       services.AddEntitiesRepositories();
       services.AddControllers();
       services.AddSwagger();
       services.AddUserService();

        var connectionString = configuration.GetConnectionString("PRLabDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing ConnectionStrings:PRLabDb. Add it to appsettings.Development.json or user-secrets.");
        }

        else logger.Log($"Conn (pre): '{connectionString}'");
        
       
        
        services.AddDbContext<PRLabPgDBContext>(options =>
            options.UseNpgsql(
                connectionString, 
                npgsql => npgsql.EnableRetryOnFailure())
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors().LogTo(
                    message => logger.Log(message),
                    LogLevel.Debug)
            );

        
        services.AddEntitiesRepositories();
  
      
    }
   
}