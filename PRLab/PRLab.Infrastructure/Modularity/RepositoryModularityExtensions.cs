using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Infrastructure.DB;
using PRLab.Infrastructure.DB.Repositories;
using PRLab.Infrastructure.DB.Seeding;

namespace PRLab.Infrastructure.Modularity;

public static class RepositoryModularityExtensions
{
    public static IServiceCollection AddEntitiesRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISeedHistoryRepository, SeedHistoryRepository>();
        
       services.AddScoped<IUserRepository, UserRepository>();
       services.AddScoped<IDescriptionRepository, DescriptionRepository>();
       services.AddScoped<IEquipmentRepository, EquipmentRepository>();
       services.AddScoped<IMuscleRepository, MuscleRepository>();
       services.AddScoped<IMovementCategoryRepository, MovementCategoryRepository>();
       
       return services; 
    }
    
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        
        return services; 
    }
    
}