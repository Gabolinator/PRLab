namespace PRLab.API.Modularity;

public static class DIExtensions
{
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "PRLab API",
                Version = "v1"
            });
        });
    }
}