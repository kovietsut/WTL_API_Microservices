using Manga.API.Extensions;
using Manga.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Start {builder.Environment.ApplicationName} up");
try
{
    builder.Host.AddAppConfigurations();
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddApplicationServices();
    builder.Services.ConfigureHealthChecks();
    builder.Services.ConfigureSwagger();
    builder.Services.ConfigureErrorCode(builder.Configuration);
    builder.Services.AddInfrastructure();
    var app = builder.Build();
    app.UseInfrastructure();
    using (var scope = app.Services.CreateScope())
    {
        var contextSeed = scope.ServiceProvider.GetRequiredService<MangaContextSeed>();
        await contextSeed.InitialiseAsync();
        await contextSeed.SeedAsync();
    }
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