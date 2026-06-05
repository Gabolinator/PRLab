using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.Modularity;
using PRLab.Infrastructure.Utilities;
using PRLab.Tools;
using PRLab.Tools.Model;

var builder = Host.CreateApplicationBuilder(args);

var logger = new PRLabLogger("PRTools", Color.Cyan);
var clock = new Clock();

builder.Configuration
    .AddJsonFile(
        Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json"),
        optional: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddUtilities(clock, logger);

builder.Services.AddInfrastructure(builder.Configuration, logger);

builder.Services.AddScoped<ToolCommandHandler>()
    .AddScoped<ToolCommandUsageLogger>()
    .AddScoped<SeedToolCommandHandler>()
    .AddScoped<ExportSeedToolCommandHandler>();

using var host = builder.Build();

using var scope = host.Services.CreateScope();

var commandHandler = scope.ServiceProvider.GetRequiredService<ToolCommandHandler>();
var config = PRToolConfig.Default();
var inputData = PRToolCommandInputData.FromInput(args, config);

await commandHandler.HandleAsync(inputData, config);