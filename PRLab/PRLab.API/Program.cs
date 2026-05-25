using PRLab.API.Modularity;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.Utilities;

var logger = new PRLabLogger("API");
logger.ToggleDecoration(false);
var clock = new Clock();

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureEnvironment(logger);
builder.ConfigureServices(logger, clock);

var app = builder.Build();
app.ConfigureRequestPipeline();
app.MapEndpoints();

await app.RunApplicationAsync(logger, clock);


namespace PRLab.API
{
    /// <summary>
    /// Partial Program class required for user secrets configuration.
    /// </summary>
    public partial class Program { }
}

