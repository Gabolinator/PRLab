namespace PRLab.API.Modularity;

public static class DIExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
       return services.AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Title = "PRLab API",
                    Version = "v1"
                });
            });
    }
}