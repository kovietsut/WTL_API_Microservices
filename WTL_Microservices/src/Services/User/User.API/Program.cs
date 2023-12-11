using Serilog;
using User.API;
using User.API.Extensions;
using User.API.Persistence;

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Start {builder.Environment.ApplicationName} up");

try
{
    builder.Host.AddAppConfigurations();
    // Add services to the container.
    builder.Services.AddAutoMapper(cfg => cfg.AddProfile(new MappingProfile()));
    builder.Services.AddInfrastructure();
    builder.Services.AddApplicationServices();
    builder.Services.ConfigureHealthChecks();
    builder.Services.ConfigureSwagger();
    builder.Services.ConfigureJWT(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    builder.Services.AddSwaggerGen();

    var app = builder.Build();
    app.UseInfrastructure();
    using (var scope = app.Services.CreateScope())
    {
        var contextSeed = scope.ServiceProvider.GetRequiredService<IdentityContextSeed>();
        await contextSeed.InitialiseAsync();
        await contextSeed.SeedAsync();
    }
    app.Run();
}
catch(Exception ex)
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