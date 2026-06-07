using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PRLab.Tools.Config;

namespace PRLab.Tools.Modularity;

public static class PRLabConfigurationExtensions
{
    public static void PRLabToolConfiguration(this HostApplicationBuilder builder, string[] args)
    {
        builder.Configuration
            .AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json"),
                optional: false)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        builder.ConfigurePRLabToolOptions();
    }

   private static void ConfigurePRLabToolOptions(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<PRToolOptions>(
            builder.Configuration.GetSection("PRTool"));
    }
}