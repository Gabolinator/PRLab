using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB.Repositories;

namespace PRLab.Infrastructure.Modularity;

public static class DIExtensions
{
    public static void AddEntitiesRepositories(this IServiceCollection services)
    {
       services.AddScoped<IDescriptionRepository, DescriptionRepository>();
    }
}