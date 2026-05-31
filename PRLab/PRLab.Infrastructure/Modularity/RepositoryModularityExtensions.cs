using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Infrastructure.DB;
using PRLab.Infrastructure.DB.Repositories;
using PRLab.Infrastructure.DB.Seeding;
using PRLab.Infrastructure.DB.Seeding.Factory;

namespace PRLab.Infrastructure.Modularity;

public static class RepositoryModularityExtensions
{
    public static void AddEntitiesRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISeedHistoryRepository, SeedHistoryRepository>();
        
       services.AddScoped<IUserRepository, UserRepository>();
       services.AddScoped<IDescriptionRepository, DescriptionRepository>();
       services.AddScoped<IEquipmentRepository, EquipmentRepository>();
       services.AddScoped<IMuscleRepository, MuscleRepository>();
       services.AddScoped<IMovementCategoryRepository, MovementCategoryRepository>();
    }
    
    public static void AddUserService(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }
    
}