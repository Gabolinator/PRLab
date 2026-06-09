using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.Modularity;

public static class DbContextModularityExtensions
{
    public static IServiceCollection AddDBContextConfiguration(this IServiceCollection services, IConfiguration configuration, IAppLogger logger)
    {
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
                .LogTo(
                Console.WriteLine,
                new[] { DbLoggerCategory.Database.Command.Name },
                LogLevel.Warning)
                // .EnableSensitiveDataLogging()
                // .EnableDetailedErrors().LogTo(
                //     message => logger.Log(message),
                //     LogLevel.Debug)
            );
        
        return services; 
    }
}