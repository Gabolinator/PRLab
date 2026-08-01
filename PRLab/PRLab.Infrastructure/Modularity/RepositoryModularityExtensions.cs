using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.UserService;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB;
using PRLab.Infrastructure.DB.Repositories;
using PRLab.Infrastructure.DB.Repositories.Entity;
using PRLab.Infrastructure.DB.Seeding;
using PRLab.Infrastructure.UserServices;
using PRLab.Infrastructure.UserServices.Authentication;

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
            .AddScoped<IMovementRepository, MovementRepository>()
            .AddScoped<IExerciseRepository, ExerciseRepository>()
            .AddScoped<IWorkoutBlockRepository, WorkoutBlockRepository>()
            .AddScoped<IWorkoutRepository, WorkoutRepository>();
       
       return services; 
    }
    
    public static IServiceCollection AddUserServices(this IServiceCollection services, bool isDevelopment ,IConfiguration configuration)
    {
        return isDevelopment ? services.AddDevelopmentUserServices(configuration) : services.AddProdUserServices(); 
    }

    private static IServiceCollection AddDevelopmentUserServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DevAuthenticationOptions>(
            configuration.GetSection("DevelopmentAuthentication"));
        
        services.AddScoped<ICurrentUserService, DevCurrentUserService>()
            .AddScoped<ISystemUserProvider, DevSystemUserProvider>()
            .AddScoped<DevelopmentUserInitializer>(); 
        
        return services;
    }
    
    private static IServiceCollection AddProdUserServices(this IServiceCollection services)
    {
        throw new NotImplementedException();
    }
    
}