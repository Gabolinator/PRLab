using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Config;
using PRLab.Infrastructure.WorkoutImport;
using PRLab.Infrastructure.WorkoutImport.Extractor;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;
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
            .AddScoped<IWorkoutImportContextFactory, WorkoutImportContextFactory>()
            .AddScoped<IWorkoutImportService, WorkoutImportService>()
            .AddSourceExtraction()
            .AddSourceInterpretation();
    }

    public static IServiceCollection AddSourceExtraction(this IServiceCollection services)
    {
        return services
            .AddScoped<IWorkoutSourceExtractor, ExcelWorkoutSourceExtractor>()
            .AddScoped<IWorkoutImportExtractorCoordinator, WorkoutImportExtractorCoordinator>();
    }
    
    public static IServiceCollection AddSourceInterpretation(this IServiceCollection services)
    {
        return services
            .AddScoped<IWorkoutImportInterpreter, CoachExcelWorkoutImportInterpreter>()
            .AddScoped<IWorkoutImportInterpreterCoordinator, WorkoutImportInterpreterCoordinator>();
    }
}