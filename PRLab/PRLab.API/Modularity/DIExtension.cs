namespace PRLab.API.Modularity;

public static class DIExtension
{
    public static void AddEntitiesRepositories(this IServiceCollection services)
    {
            // todo
    }
    
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "GainsLab API",
                Version = "v1"
            });
        });
    }
}