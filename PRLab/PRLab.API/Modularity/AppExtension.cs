

using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.API.Modularity;

public static class AppExtension
{
    
    public static void ConfigureRequestPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.ConfigureSwagger();
        }

        app.UseHttpsRedirection();
        // app.UseAuthentication();
        app.UseAuthorization();
        // app.UseCors();
    }


    public static void MapEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapGet("/healthz", HandleHealthz);
    }

    public static async Task RunApplicationAsync(this WebApplication app, IAppLogger logger, Clock clock)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PRLabPgDBContext>();
       
        db.AddLogger(logger);
        db.AddClock(clock);

        logger.Log("Migrating DB");
        await db.Database.MigrateAsync();
        logger.Log("Migrating DB - Completed");

        var pending = (await db.Database.GetPendingMigrationsAsync()).ToArray();
        logger.Log("Pending migrations: " + (pending.Length == 0 ? "none" : string.Join(", ", pending)));

        var applied = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
        logger.Log("Applied migrations: " + (applied.Length == 0 ? "none" : string.Join(", ", applied)));

        logger.Log("Can connect: " + await db.Database.CanConnectAsync());
        logger.Log("Database provider: " + db.Database.ProviderName);
        logger.Log("Connection string hash (for sanity): " + db.Database.GetDbConnection().ConnectionString.GetHashCode());
        

        //todo remove me after testing , we will seed db from the data management console.

        await app.RunAsync();
    }

    static IResult HandleHealthz() => Results.Ok(new { ok = true });
    
}