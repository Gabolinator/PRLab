using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Infrastructure.DB;
using PRLab.Infrastructure.DB.Repositories;

namespace PRLab.Infrastructure.Modularity;

public static class DIExtensions
{
    public static void AddEntitiesRepositories(this IServiceCollection services)
    {
       services.AddScoped<IUserRepository, UserRepository>();
       services.AddScoped<IDescriptionRepository, DescriptionRepository>();
       services.AddScoped<IEquipmentRepository, EquipmentRepository>();
       services.AddScoped<IMuscleRepository, MuscleRepository>();
    }
    
    public static void AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
      
    }
}