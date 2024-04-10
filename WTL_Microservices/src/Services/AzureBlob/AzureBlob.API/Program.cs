using AzureBlob.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Start {builder.Environment.ApplicationName} up");

try
{
    builder.Host.AddAppConfigurations();
    // Add services to the container.
    builder.Services.AddApplicationServices();
    builder.Services.ConfigureCors();
    builder.Services.ConfigureSwagger();
    builder.Services.ConfigureAzureBlob(builder.Configuration);
    //builder.Services.ConfigureJWT(builder.Configuration);
    builder.Services.ConfigureErrorCode(builder.Configuration);
    //builder.Services.ConfigureHealthChecks();
    builder.Services.AddHealthChecks();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    //builder.Services.ConfigureMassTransit();
    builder.Services.ConfigureRateLimtter();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();
    app.UseInfrastructure();
    app.Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal)) throw;

    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Shut down {builder.Environment.ApplicationName} complete");
    Log.CloseAndFlush();
}