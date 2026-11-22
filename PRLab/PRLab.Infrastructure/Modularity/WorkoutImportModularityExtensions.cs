using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Config;
using PRLab.Infrastructure.WorkoutImport;
using PRLab.Infrastructure.WorkoutImport.Extractor;
using PRLab.Infrastructure.WorkoutImport.Interpreter;

namespace PRLab.Infrastructure.Modularity;

public static class WorkoutImportModularityExtensions
{
    public static IServiceCollection AddWorkoutImport(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WorkoutImportOptions>(
            configuration.GetSection(
                WorkoutImportOptions.SectionName));
        
        return services
            .AddScoped<IWorkoutImportProfileProvider, JsonWorkoutImportProfileProvider>()
            .AddScoped<IWorkoutImportInterpreter, CoachWorkoutImportInterpreter>()
            .AddScoped<IWorkoutImportContextFactory, WorkoutImportContextFactory>()
            .AddScoped<IWorkoutSourceExtractor, ExcelWorkoutSourceExtractor>();
    }
}