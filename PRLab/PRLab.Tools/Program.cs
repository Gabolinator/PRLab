using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.Utilities;
using PRLab.Tools;
using PRLab.Tools.Config;
using PRLab.Tools.Model;
using PRLab.Tools.Modularity;

var builder = Host.CreateApplicationBuilder(args);

var logger = new PRLabLogger("PRTools", Color.Cyan);
var clock = new Clock();

builder.PRLabToolConfiguration(args);

builder.Services.AddPRLabToolServices(builder.Configuration, clock, logger);

using var host = builder.Build();

using var scope = host.Services.CreateScope();

var commandHandler = scope.ServiceProvider.GetRequiredService<ToolCommandHandler>();
var toolOptions = scope.ServiceProvider
    .GetRequiredService<IOptions<PRToolOptions>>()
    .Value;

var config = PRToolConfig.FromOptions(toolOptions);

var inputData = PRToolCommandInputData.FromInput(args, config);

await commandHandler.HandleAsync(inputData, config);