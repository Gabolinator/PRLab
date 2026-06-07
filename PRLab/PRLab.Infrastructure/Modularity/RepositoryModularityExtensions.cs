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
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISeedHistoryRepository, SeedHistoryRepository>()
            .AddEntitiesRepositories();
        
        return services; 
    }
    
    public static IServiceCollection AddEntitiesRepositories(this IServiceCollection services)
    {

        services.AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IDescriptionRepository, DescriptionRepository>()
            .AddScoped<IEquipmentRepository, EquipmentRepository>()
            .AddScoped<IMuscleRepository, MuscleRepository>()
            .AddScoped<IMovementCategoryRepository, MovementCategoryRepository>()
            .AddScoped<IMovementRepository, MovementRepository>();
       
       return services; 
    }
    
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        
        return services; 
    }
    
}